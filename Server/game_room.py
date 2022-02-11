import socket
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
            Gemstone.GOLDEN: 0,
            Gemstone.RUBY: 0,
            Gemstone.DIAMOND: 0,
            Gemstone.SAPPHIRE: 0,
            Gemstone.EMERALD: 0,
            Gemstone.OBSIDIAN: 0,
        }

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
        msg = message_helper.packInitResp(new_player_id, self.allocated_id)
        new_player.sendMsg(msg)

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
    def checkGetChipsLegal(self, body)->bool:
        # check all chips player want is more than ask
        for k, v in self.chips.items():
            if body[k] > self.chips[k]:
                return False

        # # check chips
        # for



        return True

    @thread_safe
    def checkBuyCardLegal(self, body)->bool:
        # TODO:
        return True

    @thread_safe
    def checkFoldCardLegal(self, body)->bool:
        # TODO:
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

        if operation_type == Operation.GET_CHIPS:
            legal = self.checkGetChipsLegal(body)
            if not legal:
                self.playerOperationInvalid(player)

            for item in operation_info:
                chip_type = item["chips_type"]
                self.chips[chip_type] += item["chips_number"]
                player.chips[chip_type] -= item["chips_number"]

        if operation_type == Operation.BUY_CARD:
            legal = self.checkBuyCardLegal(body)
            if not legal:
                self.playerOperationInvalid(player)

            card_number = operation_type["card_number"]
            card = self.card_board.getCardByNumber(card_number)
            player.addCard(card)
            self.card_board.removeCardByNumber(card_number)
            # TODO: boardcastMsg

        if operation_type == Operation.FOLD_CARD:
            pass

    @thread_safe
    def startGame(self):
        pass
