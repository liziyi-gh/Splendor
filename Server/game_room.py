import copy
import json
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
        self.chips = {
            Gemstone.GOLDEN: 5,
            Gemstone.RUBY: 0,
            Gemstone.DIAMOND: 0,
            Gemstone.SAPPHIRE: 0,
            Gemstone.EMERALD: 0,
            Gemstone.OBSIDIAN: 0,
        }
        self.players_sequence = []
        self.next_player_id = 0
        self.current_player_id = 0
        logging.debug("Game room initialize")

    def __str__(self):
        tmp_dict = {
            "players": [player.player_id for player in self.players],
            "allocated_id": self.allocated_id,
            "chips": self.chips,
            "player_sequence": self.players_sequence,
            "card_board": {
                "noble card":
                [card.number for card in self.card_board.noble_cards],
                "level 1":
                [card.number for card in self.card_board.level_one_cards],
                "level 2":
                [card.number for card in self.card_board.level_two_cards],
                "level 3":
                [card.number for card in self.card_board.level_three_cards],
            }
        }
        str = json.dumps(tmp_dict, indent=2)
        return str

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
    def checkGetChipsLegal(self, operation_info, player: Player) -> bool:
        player_chips_number = player.getAllChipsNumber()
        room_chips_number = self.getAllChipsNumber()
        operation_chips_number = 0

        for item in operation_info:
            gems_type = item["gems_type"]
            chips_number = item["gems_number"]

            operation_chips_number += chips_number
            if self.chips[gems_type] < chips_number:
                return False

            if self.chips[gems_type] < 4 and chips_number > 1:
                return False

            if chips_number > 2:
                return False

        if operation_chips_number > room_chips_number:
            return False

        if operation_chips_number > 3:
            return False

        if operation_chips_number == 0:
            return False

        if operation_chips_number == 1:
            if player_chips_number < 9 and room_chips_number > 1:
                return False

        if operation_chips_number == 2:
            if player_chips_number < 8 and len(operation_info) > 1:
                if self.getChipsLeftType() == 2:
                    return True
                return False

        if operation_chips_number == 3:
            if len(operation_info) < 3:
                return False

        return True

    @thread_safe
    def checkBuyCardLegal(self, operation_info, player: Player) -> bool:
        card_number = operation_info[0]["card_number"]
        if not self.card_board.getCardByNumber(card_number):
            if not player.cardInFold(card_number):
                return False

        for item in operation_info:
            try:
                gems_type = item["gems_type"]
                chips_number = item["gems_number"]
                if player.chips[gems_type] < chips_number:
                    return False
            except KeyError:
                pass

        return True

    @thread_safe
    def checkFoldCardLegal(self, operation_info, player: Player) -> bool:
        if len(player.fold_cards) >= 3:
            return False

        card_number = operation_info[0]["card_number"]
        if self.card_board.getCardByNumber(card_number) is None:
            return False

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
        original_msg = message_helper.packPlayerOperation(body)
        if player.player_id != self.current_player_id:
            self.playerOperationInvalid(player)
            return

        if operation_type == Operation.GET_GEMS:
            legal = self.checkGetChipsLegal(operation_info, player)
            if not legal:
                self.playerOperationInvalid(player)
                return

            for item in operation_info:
                chip_type = item["gems_type"]
                chip_number = int(item["gems_number"])
                self.chips[chip_type] -= chip_number
                player.chips[chip_type] += chip_number

            self.boardcastMsg(original_msg)
            self.startNewTurn()

            # TODO: check chips > 10

            return

        if operation_type == Operation.FOLD_CARD:
            # FIXME: if could have golden, but have to discard
            legal = self.checkFoldCardLegal(operation_info, player)
            if not legal:
                self.playerOperationInvalid(player)
                return
            card_number = operation_info[0]["card_number"]
            card = self.card_board.getCardByNumber(card_number)
            player.addFoldCard(card)
            new_card_number = self.card_board.removeCardByNumberThenAddNewCard(
                card_number)

            # FIXME: if fold 10001 like card should not let other players know
            new_body = copy.deepcopy(body)
            if self.chips[Gemstone.GOLDEN] > 0:
                golden_dict = {"golden_number": 1}
                player.chips[Gemstone.GOLDEN] += 1
                self.chips[Gemstone.GOLDEN] -= 1
            else:
                golden_dict = {{"golden_number": 0}}
            new_body["operation_info"].append(golden_dict)
            msg = message_helper.packPlayerOperation(new_body)
            new_card_msg = message_helper.packNewCard(player.player_id,
                                                      new_card_number)

            self.boardcastMsg(msg)
            self.boardcastMsg(new_card_msg)
            self.startNewTurn()

            return

        if operation_type == Operation.BUY_CARD:
            in_fold = False
            legal = self.checkBuyCardLegal(operation_info, player)
            if not legal:
                self.playerOperationInvalid(player)
                return

            self.boardcastMsg(original_msg)

            card_number = operation_info[0]["card_number"]
            card = self.card_board.getCardByNumber(card_number)
            if card is None:
                card = player.getCardInFoldCards(card_number)
                in_fold = True
            player.addCard(card, operation_info)

            try:
                for item in operation_info[1:]:
                    gems_type = item["gems_type"]
                    gems_number = item["gems_number"]
                    self.chips[gems_type] += gems_number
            except KeyError:
                print("key error")

            if not in_fold:
                new_card_number = self.card_board.removeCardByNumberThenAddNewCard(
                    card_number)
                new_card_msg = message_helper.packNewCard(player.player_id,
                                                          new_card_number)
                self.boardcastMsg(new_card_msg)


        available_cards = self.card_board.checkAvailbaleNobleCard(player)
        if len(available_cards) > 0:
            if len(available_cards) == 1:
                card = available_cards[0]
                player.addCard(card)
                self.card_board.removeCardByNumberThenAddNewCard(card.number)
                msg = message_helper.packPlayerGetNoble(player.player_id, card)
                self.boardcastMsg(msg)
                self.startNewTurn()

                return

            if len(available_cards) > 1:
                msg = message_helper.packAskPlayerGetNoble(
                    player.player_id, available_cards)
                player.sendMsg(msg)

                return
        else:
            self.startNewTurn()

    @thread_safe
    def doPlayerGetNoble(self, header: Header, body):
        player = self.findPlayerByID(header.player_id)
        card_number = body["noble_number"]
        card = self.card_board.getCardByNumber(card_number)
        player.addCard(card)
        self.card_board.removeCardByNumberThenAddNewCard(card_number)
        self.startNewTurn()

    @thread_safe
    def startGame(self):
        self.generatePlayerSequence()
        self.current_player_id = self.players_sequence[0]
        self.next_player_id = self.players_sequence[0]
        players_number = len(self.allocated_id)
        chips_num = 3
        if players_number == 2:
            chips_num = 4
        if players_number == 3:
            chips_num = 5
        if players_number == 4:
            chips_num = 7
        for key in self.chips.keys():
            if key == Gemstone.GOLDEN:
                continue
            self.chips[key] = chips_num

        msg = message_helper.packGameStart(players_number,
                                           self.players_sequence,
                                           self.card_board)
        self.boardcastMsg(msg)
        logging.info("Game start!")
        self.startNewTurn()

    @thread_safe
    def startNewTurn(self):
        self.current_player_id = self.next_player_id
        msg = message_helper.packNewTurn(self.next_player_id)
        self.boardcastMsg(msg)
        logging.info("Start new turn with player {}".format(
            self.next_player_id))

        idx = self.players_sequence.index(self.next_player_id)
        idx = 0 if idx == len(self.players_sequence) - 1 else idx + 1
        self.next_player_id = self.players_sequence[idx]

    @thread_safe
    def getAllChipsNumber(self) -> int:
        ans = 0
        for _, v in self.chips.items():
            ans += v

        ans -= self.chips[Gemstone.GOLDEN]
        return ans

    @thread_safe
    def getChipsLeftType(self) -> int:
        ans = 0
        for k, v in self.chips.items():
            if v > 0 and k != Gemstone.GOLDEN:
                ans += 1

        return ans
