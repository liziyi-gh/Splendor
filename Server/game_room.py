import socket
import random
import logging

from Server import message_helper
from Server.api_id import API_ID
from Server.gemstone import Gemstone
from Server.constants import Header
from Server.player import Player
from Server.operation import Operation
from Server.card_board import CardBoard
from Server.func_helper import thread_safe


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
        self.players_sequence = []
        logging.debug("Game room initialize")

    @thread_safe
    def generatePlayerSequence(self):
        random.shuffle(self.allocated_id)
        self.players_sequence = self.allocated_id

    @thread_safe
    def findPlayerByID(self, player_id) -> Player:
        for player in self.players:
            if player.player_id == player_id:
                return player

        logging.error("Could not find player by ID {}".format(player_id))

    @thread_safe
    def newPlayerID(self) -> int:
        for i in self.allow_player_id:
            if i not in self.allocated_id:
                self.allocated_id.append(i)
                return i

        logging.error("Could not allocated new player ID")

    @thread_safe
    def addPlayer(self, sock: socket.socket):
        new_player_id = self.newPlayerID()
        new_player = Player(sock, new_player_id)
        self.players.append(new_player)
        for key in self.chips.keys():
            if key not in Gemstone.GOLDEN:
                self.chips[key] += 1
        self.card_board.addPlayer()
        init_resp_msg = message_helper.packInitResp(new_player_id,
                                                    self.allocated_id)
        new_player.sendMsg(init_resp_msg)
        new_player_msg = message_helper.packNewPlayer(new_player_id)
        self.boardcastMsg(new_player_msg, API_ID.NEW_PLAYER)

    @thread_safe
    def boardcastMsg(self, msg, api_id=None):
        for player in self.players:
            player.sendMsg(msg)

        logging.debug("Finish boardcast msg, api id {}".format(api_id))

    @thread_safe
    def playerReady(self, header: Header, body):
        player_id = header.player_id
        player = self.findPlayerByID(player_id)
        player.setReady()
        msg = message_helper.packPlayerReady(player_id)
        self.boardcastMsg(msg)
        if len(self.players) > 1:
            for player in self.players:
                if not player.ready:
                    return

        self.startGame()

    @thread_safe
    def checkGetChipsLegal(self, operation_info) -> bool:
        for item in operation_info:
            gems_type = operation_info["gems_type"]
            if self.chips[gems_type] < item[gems_type]:
                return False

            if self.chips[gems_type] < 4 and item[gems_type] > 1:
                return False

        return True

    @thread_safe
    def checkBuyCardLegal(self, operation_info, player: Player) -> bool:
        card_number = operation_info[0]["card_number"]
        if not self.card_board.getCardByNumber(card_number):
            return False

        for item in operation_info:
            try:
                gems_type = item["gems_type"]
                if player.chips[gems_type] < item["gems_number"]:
                    return False
            except KeyError:
                pass
        return True

    @thread_safe
    def checkFoldCardLegal(self, operation_info, player: Player) -> bool:
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
    def playerOperationInvalid(self, player: Player):
        logging.debug("Player {} Operation invalid".format(player.player_id))
        msg = message_helper.packPlayerOperationInvalid(player.player_id)
        player.sendMsg(msg)

    @thread_safe
    def doPlayerOperation(self, header: Header, body):
        player = self.findPlayerByID(header.player_id)
        operation_type = body["operation_type"]
        operation_info = body["operation_info"]
        operation_msg = message_helper.packPlayerOperation(body)

        if operation_type == Operation.GET_GEMS:
            legal = self.checkGetChipsLegal(operation_info)
            if not legal:
                self.playerOperationInvalid(player)

            for item in operation_info:
                chip_type = item["chips_type"]
                chip_number = item["chips_number"]
                self.chips[chip_type] += chip_number
                player.chips[chip_type] -= chip_number

            self.boardcastMsg(operation_msg)
            self.startNewTurn(player.player_id)

            return

        if operation_type == Operation.BUY_CARD:
            legal = self.checkBuyCardLegal(operation_info, player)
            if not legal:
                self.playerOperationInvalid(player)
                return

            card_number = operation_type["card_number"]
            card = self.card_board.getCardByNumber(card_number)
            player.addCard(card, operation_info)
            self.card_board.removeCardByNumberThenAddNewCard(card_number)

            self.boardcastMsg(operation_msg)
            self.startNewTurn(player.player_id)

            return

        if operation_type == Operation.FOLD_CARD:
            legal = self.checkFoldCardLegal(operation_info, player)
            if not legal:
                self.playerOperationInvalid(player)
                return
            card_number = operation_type["card_number"]
            card = self.card_board.getCardByNumber(card_number)
            player.addFoldCard(card)
            self.card_board.removeCardByNumberThenAddNewCard(card_number)

            self.boardcastMsg(operation_msg)
            self.startNewTurn(player.player_id)

            return

        available_cards = self.card_board.checkAvailbaleNobleCard(player)
        if len(available_cards) > 0:
            if len(available_cards) == 1:
                card = available_cards[0]
                player.addCard(card)
                self.card_board.removeCardByNumberThenAddNewCard(card.number)
                msg = message_helper.packPlayerGetNoble(player.player_id, card)
                self.boardcastMsg(msg)
                self.startNewTurn(player.player_id)

                return

            if len(available_cards) > 1:
                msg = message_helper.packAskPlayerGetNoble(
                    player.player_id, available_cards)
                player.sendMsg(msg)

                return

    @thread_safe
    def doPlayerGetNoble(self, header: Header, body):
        player = self.findPlayerByID(header.player_id)
        card_number = body["noble_number"]
        card = self.card_board.getCardByNumber(card_number)
        player.addCard(card)
        self.card_board.removeCardByNumberThenAddNewCard(card_number)
        self.startNewTurn(player.player_id)

    @thread_safe
    def startGame(self):
        self.generatePlayerSequence()
        players_number = len(self.allocated_id)
        msg = message_helper.packGameStart(players_number,
                                           self.players_sequence,
                                           self.card_board)
        self.boardcastMsg(msg)
        logging.info("Game start!")
        self.startNewTurn(self.players_sequence[0])

    @thread_safe
    def startNewTurn(self, player_id):
        idx = self.players_sequence.index(player_id)
        idx = 0 if idx == len(self.players_sequence) - 1 else idx + 1
        next_player_id = self.players_sequence[idx]
        msg = message_helper.packNewTurn(next_player_id)
        self.boardcastMsg(msg)
        logging.info("Start new turn with player {}".format(next_player_id))
