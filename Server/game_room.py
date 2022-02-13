import socket
import random
import threading

from Server import message_helper
from Server.gemstone import Gemstone
from Server.message_helper import Header
from Server.player import Player
from Server.operation import Operation
from Server.card_board import CardBoard

def static_vars(**kwargs):

    def decorate(func):
        for k in kwargs:
            setattr(func, k, kwargs[k])
        return func

    return decorate

def thread_safe(function):

    @static_vars(lock=threading.Lock())
    def async_wrapper(self, *args, **kwargs):
        with async_wrapper.lock:
            ret = function(self, *args, **kwargs)
            return ret

    return async_wrapper


class GameRoom:
    def __init__(self) -> None:
        self.players = []
        self.allocated_id = []
        self.allow_player_id = (1, 2, 3, 4)
        self.card_board = CardBoard()
        self.last_operation = {}
        self.chips = {
            Gemstone.GOLDEN: 5,
            Gemstone.RUBY: 2,
            Gemstone.DIAMOND: 2,
            Gemstone.SAPPHIRE: 2,
            Gemstone.EMERALD: 2,
            Gemstone.OBSIDIAN: 2,
        }
        self.players_sequence = None

    @thread_safe
    def generatePlayerSequence(self):
        self.players_sequence = random.shuffle(self.allocated_id)

    @thread_safe
    def findPlayerByID(self, player_id)->Player:
        for player in self.players:
            if player.player_id == player_id:
                return player

    @thread_safe
    def newPlayerID(self)-> int:
        for i in self.allow_player_id:
            if i not in self.allocated_id:
                self.allocated_id.append(i)
                return i

    @thread_safe
    def addPlayer(self, sock:socket.socket):
        new_player_id = self.newPlayerID()
        new_player = Player(sock, new_player_id)
        self.players.append(new_player)
        for key in self.chips.keys():
            if key not in Gemstone.GOLDEN:
                self.chips[key] += 1
        self.card_board.addPlayer()
        msg = message_helper.packInitResp(new_player_id, self.allocated_id)
        new_player.sendMsg(msg)
        # TODO: boardcastMsg

    @thread_safe
    def boardcastMsg(self, msg):
        # TODO: may need Multithreading
        for player in self.players:
            player.sendMsg(msg)

    @thread_safe
    def playerReady(self, header:Header, body):
        # TODO: test 这个函数调用了广播函数之后，会不会死锁，以及上下文管理器的语义
        player_id = header.player_id
        player = self.findPlayerByID(player_id)
        player.setReady()
        msg = message_helper.packPlayerReady(player_id)
        self.boardcastMsg(msg)

    @thread_safe
    def checkGetChipsLegal(self, operation_info)->bool:
        for item in operation_info:
            gems_type = operation_info["gems_type"]
            if self.chips[gems_type] < item[gems_type]:
                return False

            if self.chips[gems_type] < 4 and item[gems_type] > 1:
                return False

        return True

    @thread_safe
    def checkBuyCardLegal(self, operation_info, player:Player)->bool:
        card_number = operation_info[0]["card_number"]
        # TODO: check card_number in self.card_board
        for item in operation_info:
            try:
                gems_type = item["gems_type"]
                if player.chips[gems_type] < item["gems_number"]:
                    return False
            except KeyError:
                pass
        return True

    @thread_safe
    def checkFoldCardLegal(self, operation_info, player:Player)->bool:
        if len(player.fold_cards) >= 3:
            return False

        try:
            if operation_info[1]["gems_type"] != Gemstone.GOLDEN:
                return False

            gold_number = operation_info[1]["gems_number"]
            if gold_number > 1:
                return False
            if gold_number == 1 and self.chips[Gemstone.GOLDEN] < 1:
                return False

        except KeyError:
            pass

        return True

    @thread_safe
    def playerOperationInvalid(self, player:Player):
        msg = message_helper.packPlayerOperationInvalid(player.player_id)
        player.sendMsg(msg)

    @thread_safe
    def doPlayerOperation(self, header:Header, body):
        player = self.findPlayerByID(header.player_id)
        operation_type = body["operation_type"]
        operation_info = body["operation_info"]

        if operation_type == Operation.GET_GEMS:
            legal = self.checkGetChipsLegal(operation_info)
            if not legal:
                self.playerOperationInvalid(player)

            for item in operation_info:
                chip_type = item["chips_type"]
                chip_number = item["chips_number"]
                self.chips[chip_type] += chip_number
                player.chips[chip_type] -= chip_number

        if operation_type == Operation.BUY_CARD:
            legal = self.checkBuyCardLegal(operation_info, player)
            if not legal:
                self.playerOperationInvalid(player)
                return

            card_number = operation_type["card_number"]
            card = self.card_board.getCardByNumber(card_number)
            player.addCard(card, operation_info)
            self.card_board.removeCardByNumberThenAddNewCard(card_number)
            # TODO: boardcastMsg

        if operation_type == Operation.FOLD_CARD:
            legal = self.checkFoldCardLegal(operation_info, player)
            if not legal:
                self.playerOperationInvalid(player)
                return
            card_number = operation_type["card_number"]
            card = self.card_board.getCardByNumber(card_number)
            player.addFoldCard(card)
            self.card_board.removeCardByNumberThenAddNewCard(card_number)
            # TODO: boardcastMsg

        # TODO: New Turn

    @thread_safe
    def startGame(self):
        self.generatePlayerSequence()
        players_number = len(self.allocated_id)
        msg = message_helper.packGameStart(players_number, self.players_sequence,
                                           self.card_board)
        self.boardcastMsg(msg)
