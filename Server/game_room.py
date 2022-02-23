import copy
import json
import socket
import random
import logging

from Server import message_helper
from Server.api_id import API_ID
from Server.gemstone import Gemstone
from Server.constants import Header, HEADER_LENGTH
from Server.player import Player
from Server.operation import Operation
from Server.card_board import CardBoard
from Server.func_helper import thread_safe
from Server.message_helper import unpack_header, unpack_body
from Server import client_socket


class ClientDisconnect(RuntimeError):
    pass


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
        self.current_expected_operation = None
        self.started = False
        self.last_turn = False
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

    def check_game_stop(self) -> bool:
        is_last_player = self.current_player_id == self.players_sequence[-1]
        if self.last_turn and is_last_player:
            return True

        for player in self.players:
            if player.points >= 15:
                self.last_turn == True
                if is_last_player:
                    return True
                else:
                    return False

        return False

    def find_winner(self) -> int:
        possible_winners = []
        for player in self.players:
            if player.points >= 15:
                possible_winners.append(player)

        min_cards = 10000  # a number bigger than all cards
        winner = 0
        for player in possible_winners:
            if len(player.cards) < min_cards:
                winner = player

        return winner.player_id

    @thread_safe
    def generate_player_sequence(self):
        random.shuffle(self.allocated_id)
        self.players_sequence = self.allocated_id

    @thread_safe
    def find_player_by_ID(self, player_id) -> Player:
        for player in self.players:
            if player.player_id == player_id:
                return player

        logging.error("Could not find player by ID {}".format(player_id))

    @thread_safe
    def new_player_ID(self) -> int:
        for i in self.allow_player_id:
            if i not in self.allocated_id:
                self.allocated_id.append(i)
                return i

        logging.error("Could not allocated new player ID")

        return -1

    @thread_safe
    def add_player(self, sock: socket.socket):
        new_player_id = self.new_player_ID()
        if new_player_id < 0:
            return False
        new_player = Player(sock, new_player_id)
        self.players.append(new_player)
        self.card_board.add_player()
        init_resp_msg = message_helper.pack_init_resp(new_player_id,
                                                    self.allocated_id)
        new_player.send_msg(init_resp_msg)
        new_player_msg = message_helper.pack_new_player(new_player_id)
        self.boardcast_msg(new_player_msg, API_ID.NEW_PLAYER)

    @thread_safe
    def boardcast_msg(self, msg, api_id=None):
        for player in self.players:
            player.sendMsg(msg)

        logging.debug("Finish boardcast msg, api id {}".format(api_id))

    @thread_safe
    def boardcast_diffrent_msg(self,
                             msg,
                             special_msg,
                             special_player: Player,
                             api_id=None):
        for player in self.players:
            if player is not special_player:
                player.sendMsg(msg)

        special_player.send_msg(special_msg)
        logging.debug("Finish boardcast msg, api id {}".format(api_id))

    @thread_safe
    def player_ready(self, header: Header):
        player_id = header.player_id
        player = self.find_player_by_ID(player_id)
        player.set_ready()
        msg = message_helper.pack_player_ready(player_id)
        self.boardcast_msg(msg, API_ID.PLAYER_READY)
        if len(self.players) > 1:
            for player in self.players:
                if not player.ready:
                    return
            self.start_game()

    @thread_safe
    def check_get_chips_legal(self, operation_info, player: Player) -> bool:
        player_chips_number = player.get_all_chips_number()
        room_chips_number = self.get_all_chips_number()
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
                if self.get_chips_left_type() == 2:
                    return True
                return False

        if operation_chips_number == 3:
            if len(operation_info) < 3:
                return False

        return True

    @thread_safe
    def check_discard_gems_legal(self, operation_info, player: Player) -> bool:
        discard_numbers = 0
        for k, v in operation_info.items():
            discard_numbers += v
            if player.chips[k] < v:
                return False

        if player.get_all_chips_number() - discard_numbers != 10:
            return False

        return True

    @thread_safe
    def check_buy_card_legal(self, operation_info, player: Player) -> bool:
        card_number = operation_info[0]["card_number"]

        card = self.card_board.get_card_by_number(card_number)
        if card is None:
            card = player.get_card_in_fold_cards(card_number)
            if card is None:
                return False

        operation_chips_dict = {
            item["gems_type"]: item["gems_number"]
            for item in operation_info[1:]
        }
        need_golden_number = 0
        logging.debug("player chips is")
        logging.debug(json.dumps(player.chips))
        for k, needed_chip in card.chips.items():
            op_chips = 0
            player_gems = player.get_gemstone_number(k)
            logging.debug("player {} number is {}".format(k, player_gems))
            try:
                op_chips = operation_chips_dict[k]
            except KeyError:
                op_chips = 0

            if op_chips < 0:
                return False

            if needed_chip == 0 and op_chips > 0:
                logging.info("extra {} buying card".format(k))
                return False

            if needed_chip > 0:
                if op_chips > 0 and needed_chip < op_chips + player_gems:
                    logging.info("too much {} buying card".format(k))
                    return False

                if op_chips == 0 and needed_chip <= player_gems:
                    continue

                logging.debug("need {} number is {}".format(k, needed_chip))
                tmp = needed_chip - op_chips - player_gems
                if tmp > 0:
                    need_golden_number = need_golden_number + tmp

        logging.debug("need golden number is {}".format(need_golden_number))

        try:
            if need_golden_number < operation_chips_dict[Gemstone.GOLDEN]:
                logging.info("too much chips to buy card")
                return False

            if need_golden_number > operation_chips_dict[Gemstone.GOLDEN]:
                logging.info("need more chips to buy card")
                return False

        except KeyError:
            if need_golden_number > 0:
                logging.info("need more chips to buy card")
                return False

        if need_golden_number < 0:
            logging.info("too much chips to buy card")
            return False

        return True

    @thread_safe
    def check_fold_card_legal(self, operation_info, player: Player) -> bool:
        if len(player.fold_cards) >= 3:
            logging.info(
                "fold card illegal, player already have {} fold cards".format(
                    len(player.fold_cards)))
            return False

        card_number = operation_info[0]["card_number"]
        if self.card_board.get_card_by_number(card_number) is None:
            logging.info(
                "fold card illegal, can not find card {} in card board".format(
                    card_number))
            return False

        return True

    @thread_safe
    def player_operation_invalid(self, player: Player):
        logging.debug("Player {} Operation invalid".format(player.player_id))
        msg = message_helper.pack_player_operation_invalid(player.player_id)
        player.send_msg(msg)

    @thread_safe
    def do_player_operation(self, header: Header, body):
        player = self.find_player_by_ID(header.player_id)
        operation_type = body["operation_type"]
        operation_info = body["operation_info"]
        original_msg = message_helper.pack_player_operation(body)

        if player.player_id != self.current_player_id:
            self.player_operation_invalid(player)
            logging.error("Current turn is palyer {}".format(
                self.current_player_id))
            return

        if self.current_expected_operation is not None and operation_type != self.current_expected_operation:
            logging.debug("Expecting operation is {}".format(
                self.current_expected_operation))
            self.player_operation_invalid(player)
            return

        if operation_type == Operation.GET_GEMS:
            if self.try_get_gems(player, operation_info, original_msg):
                if not self.check_chips_number_legal(player):
                    return
                if not self.check_available_noble_cards(player):
                    return
            else:
                return

        if operation_type == Operation.FOLD_CARD:
            if self.try_fold_card(player, operation_info, body):
                if not self.check_chips_number_legal(player):
                    return
                if not self.check_available_noble_cards(player):
                    return
            else:
                return

        if operation_type == Operation.FOLD_CARD_UNKNOWN:
            if self.try_fold_card_unknown(player, operation_info, body):
                if not self.check_chips_number_legal(player):
                    return
                if not self.check_available_noble_cards(player):
                    return
            else:
                return

        if operation_type == Operation.DISCARD_GEMS:
            if self.try_discard_gems(player, operation_info, body):
                if not self.check_available_noble_cards(player):
                    return
            else:
                return

        if operation_type == Operation.BUY_CARD:
            if self.try_buy_card(player, operation_info, original_msg):
                if not self.check_available_noble_cards(player):
                    return
            else:
                return

        if self.check_game_stop():
            winner_id = self.find_winner()
            msg = message_helper.pack_winner(winner_id)
            logging.info("player {} win".format(msg, API_ID.WINNER))
            self.boardcast_msg(msg)

            return

        self.start_new_turn()

    @thread_safe
    def do_player_get_noble(self, header: Header, body):
        player = self.find_player_by_ID(header.player_id)
        card_number = body["noble_number"][0]
        card = self.card_board.get_card_by_number(card_number)

        if not player.check_availbale_noble_card(card):
            self.player_operation_invalid(player)
            return False

        player.add_card(card)
        self.card_board.remove_card_by_number_then_add_new_card(card_number)
        msg = message_helper.pack_universial(body, API_ID.PLAYER_GET_NOBLE)
        self.boardcast_msg(msg)

        if self.check_game_stop():
            winner_id = self.find_winner()
            msg = message_helper.pack_winner(winner_id)
            logging.info("player {} win the game".format(msg))
            self.boardcast_msg(msg, API_ID.WINNER)

            return

        self.start_new_turn()

    @thread_safe
    def start_game(self):
        self.generate_player_sequence()
        self.current_player_id = self.players_sequence[0]
        self.next_player_id = self.players_sequence[0]
        players_number = len(self.allocated_id)
        chips_num_dict = {2: 4, 3: 5, 4: 7}
        chips_num = chips_num_dict[players_number]
        for key in self.chips.keys():
            if key == Gemstone.GOLDEN:
                continue
            self.chips[key] = chips_num

        msg = message_helper.pack_game_start(players_number,
                                             self.players_sequence,
                                             self.card_board)
        self.boardcast_msg(msg)
        self.started = True
        logging.info("Game start!")
        self.start_new_turn()

    @thread_safe
    def start_new_turn(self):
        self.current_player_id = self.next_player_id
        msg = message_helper.pack_new_turn(self.next_player_id)
        self.boardcast_msg(msg)
        logging.info("Start new turn with player {}".format(
            self.next_player_id))

        idx = self.players_sequence.index(self.next_player_id)
        idx = 0 if idx == len(self.players_sequence) - 1 else idx + 1
        self.next_player_id = self.players_sequence[idx]

    @thread_safe
    def get_all_chips_number(self) -> int:
        ans = 0
        for _, v in self.chips.items():
            ans += v

        ans -= self.chips[Gemstone.GOLDEN]
        return ans

    @thread_safe
    def get_chips_left_type(self) -> int:
        ans = 0
        for k, v in self.chips.items():
            if v > 0 and k != Gemstone.GOLDEN:
                ans += 1

        return ans

    def try_discard_gems(self, player, operation_info, body) -> bool:
        legal = self.check_discard_gems_legal(operation_info, player)
        if not legal:
            self.player_operation_invalid(player)
            return False

        for k, v in operation_info.items():
            player.chips[k] -= v
            self.chips[k] += v

        msg = message_helper.pack_player_operation(body)
        self.boardcast_msg(msg)
        self.current_expected_operation = None
        return True

    def try_get_gems(self, player, operation_info, original_msg) -> bool:
        legal = self.check_get_chips_legal(operation_info, player)
        if not legal:
            self.player_operation_invalid(player)
            return False

        for item in operation_info:
            chip_type = item["gems_type"]
            chip_number = item["gems_number"]
            self.chips[chip_type] -= chip_number
            player.chips[chip_type] += chip_number

        self.boardcast_msg(original_msg)

        return True

    def check_chips_number_legal(self, player: Player) -> bool:
        new_turn = True
        player_chips_number = player.get_all_chips_number()
        if player_chips_number > 10:
            msg = message_helper.pack_discard_gems(player.player_id,
                                                 player_chips_number - 10)
            player.send_msg(msg)
            self.current_expected_operation = Operation.DISCARD_GEMS
            new_turn = False

        return new_turn

    def try_fold_card(self, player: Player, operation_info,
                             body) -> bool:
        legal = self.check_fold_card_legal(operation_info, player)
        if not legal:
            self.player_operation_invalid(player)
            return False
        card_number = operation_info[0]["card_number"]
        card = self.card_board.get_card_by_number(card_number)
        player.add_fold_card(card)
        new_card_number = self.card_board.remove_card_by_number_then_add_new_card(
            card_number)

        new_body = copy.deepcopy(body)
        if self.chips[Gemstone.GOLDEN] > 0:
            golden_dict = {"golden_number": 1}
            player.chips[Gemstone.GOLDEN] += 1
            self.chips[Gemstone.GOLDEN] -= 1
        else:
            golden_dict = {{"golden_number": 0}}

        new_body["operation_info"].append(golden_dict)
        msg = message_helper.pack_player_operation(new_body)
        self.boardcast_msg(msg)

        if new_card_number != 0:
            new_card_msg = message_helper.pack_new_card(player.player_id,
                                                      new_card_number)
            self.boardcast_msg(new_card_msg)

        return True

    def try_buy_card(self, player: Player, operation_info,
                            original_msg) -> bool:
        in_fold = False
        legal = self.check_buy_card_legal(operation_info, player)
        if not legal:
            self.player_operation_invalid(player)
            return False

        self.boardcast_msg(original_msg)

        card_number = operation_info[0]["card_number"]
        card = self.card_board.get_card_by_number(card_number)
        if card is None:
            card = player.get_card_in_fold_cards(card_number)
            in_fold = True

        for item in operation_info[1:]:
            gems_type = item["gems_type"]
            gems_number = item["gems_number"]
            self.chips[gems_type] += gems_number
            player.chips[gems_type] -= gems_number

        player.add_card(card)

        if not in_fold:
            new_card_number = self.card_board.remove_card_by_number_then_add_new_card(card_number)
            if new_card_number != 0:
                new_card_msg = message_helper.pack_new_card(player.player_id, new_card_number)
                self.boardcast_msg(new_card_msg)

        return True

    def check_available_noble_cards(self, player: Player) -> bool:
        new_turn = True
        available_cards = [
            noble_card for noble_card in self.card_board.noble_cards
            if player.check_availbale_noble_card(noble_card)
        ]
        if len(available_cards) > 0:
            logging.debug("Available noble cards > 0")
            if len(available_cards) == 1:
                card = available_cards[0]
                player.add_card(card)
                self.card_board.remove_card_by_number_then_add_new_card(card.number)
                msg = message_helper.pack_player_get_noble(player.player_id, card)
                logging.info("Player {} get noble card {}".format(
                    player.player_id, card.number))
                self.boardcast_msg(msg)

            if len(available_cards) > 1:
                msg = message_helper.pack_ask_player_get_noble(
                    player.player_id, available_cards)
                player.send_msg(msg)
                new_turn = False
        else:
            logging.debug("No available noble cards")

        return new_turn

    def check_fold_unknown_card_legal(self, player: Player, card_number) -> bool:
        if len(player.fold_cards) >= 3:
            logging.info(
                "fold card illegal, player already have {} fold cards".format(
                    len(player.fold_cards)))
            return False

        return card_number in [10001, 10002, 10003]

    def try_fold_card_unknown(self, player: Player, operation_info,
                                   body) -> bool:
        card_number = operation_info[0]["card_number"]
        card_level = card_number - 10000
        legal = self.check_fold_unknown_card_legal(player, card_number)

        if not legal:
            self.player_operation_invalid(player)
            return False

        # this because card_board.nextCardInRepo has side effect
        new_card = self.card_board.get_next_card_in_repo(card_level)

        if new_card.card_type is None:
            self.player_operation_invalid(player)
            return False

        player.add_fold_card(new_card)
        new_body = copy.deepcopy(body)

        if self.chips[Gemstone.GOLDEN] > 0:
            golden_number = 1
            player.chips[Gemstone.GOLDEN] += 1
            self.chips[Gemstone.GOLDEN] -= 1
        else:
            golden_number = 0

        new_body["operation_info"][0]["golden_number"] = golden_number
        new_card_msg = message_helper.pack_player_operation(new_body)

        new_body["operation_info"][0]["card_number"] = new_card.number
        real_new_card_msg = message_helper.pack_player_operation(new_body)

        self.boardcast_diffrent_msg(new_card_msg, real_new_card_msg, player,
                                  API_ID.NEW_CARD)

        return True

    def receive_msg(self, client_sock: socket.socket):
        try:
            header_data = client_sock.recv(HEADER_LENGTH)
        except ConnectionResetError as e:
            logging.info("socket {} reset connection".format(client_sock))
            client_sock.close()

            raise ClientDisconnect() from e

        if len(header_data) < HEADER_LENGTH:
            logging.error("header data length less than {}".format(HEADER_LENGTH))
            client_socket.remove(client_sock)
            client_sock.close()

            raise ClientDisconnect()

        header = unpack_header(header_data)
        msg_body_len = header.msg_len - HEADER_LENGTH
        logging.debug("Receive new msg, api id {}, msg length {}".format(header.api_id, header.msg_len))
        body = None
        if msg_body_len > 0:
            body_data = client_sock.recv(msg_body_len)
            body = unpack_body(body_data)
            logging.debug("body is {}".format(body_data.decode()))

        return header, body

    def handle_client(self, client_sock: socket.socket, addr):
        logging.info("Start handling new socket, addr is {}".format(addr))
        while True:
            try:
                header, body = self.receive_msg(client_sock)
            except ConnectionResetError:
                return
            # TODO: check player_id match socket?
            # TODO: what if socket change?
            if header.api_id == API_ID.INIT:
                self.add_player(client_sock)

            if header.api_id == API_ID.PLAYER_READY:
                self.player_ready(header)

            if header.api_id == API_ID.PLAYER_OPERATION:
                self.do_player_operation(header, body)

            if header.api_id == API_ID.PLAYER_GET_NOBLE:
                self.do_player_get_noble(header, body)

            logging.debug("current game room info is:")
            logging.debug(str(self))
