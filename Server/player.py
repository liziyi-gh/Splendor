import json
import logging
import socket
import struct
from Server import allocated_id

from Server.constants import HEADER_FORMAT, HEADER_LENGTH
from Server.gemstone import Gemstone
from Server.card import Card


class Player:

    def __init__(self, sock: socket.socket, player_id: int):
        self.sock = sock
        self.ready = False
        self.player_id = player_id
        self.points = 0
        self.cards = []
        self.fold_cards = []

        self.ruby = 0
        self.diamond = 0
        self.sapphire = 0
        self.emerald = 0
        self.obsidian = 0
        self.golden = 0

        self.chips = {
            Gemstone.GOLDEN: 0,
            Gemstone.RUBY: 0,
            Gemstone.DIAMOND: 0,
            Gemstone.SAPPHIRE: 0,
            Gemstone.EMERALD: 0,
            Gemstone.OBSIDIAN: 0,
        }

    def __str__(self):
        tmp = self.__dict__
        tmp.pop("sock")

        # FIXME: method will in tmp?
        return json.dumps(tmp)

    def __del__(self):
        allocated_id.remove_player_id(self.player_id)

    def send_msg(self, msg) -> None:
        self.sock.send(msg)
        header_data = struct.unpack(HEADER_FORMAT, msg[0:HEADER_LENGTH])
        logging.debug(f"Send msg to player {self.player_id}, api {header_data[0]}, body is")
        if header_data[2] > HEADER_LENGTH:
            body_data = msg[HEADER_LENGTH:]
            body = json.loads(body_data.decode())
            logging.debug(body)

    def set_ready(self):
        logging.info(f"Player {self.player_id} ready")
        self.ready = True

    def add_card(self, card: Card) -> None:
        if card.level == 0:
            # Noble card
            self.points += card.points
        else:
            # Normal card
            if card in self.fold_cards:
                self.fold_cards.remove(card)

            setattr(self, card.gem_type, getattr(self, card.gem_type) + 1)
            self.points += card.points

        self.cards.append(card)
        logging.info(f"Player {self.player_id} got card {card.number}")

    def add_fold_card(self, card: Card) -> None:
        self.fold_cards.append(card)
        logging.info(f"Player {self.player_id} fold card {card.number}")

    def check_availbale_noble_card(self, card: Card) -> bool:
        for gemstone in card.chips.keys():
            gem_numbers = getattr(self, gemstone)
            if gem_numbers < card.chips[gemstone]:
                return False

        return True

    def get_all_chips_number(self) -> int:
        ans = 0
        for _, v in self.chips.items():
            ans += v

        return ans

    def card_in_fold(self, card_number) -> bool:
        for card in self.fold_cards:
            if card.number == card_number:
                logging.debug(
                    f"Find card in player {self.player_id} fold card by number {card_number}")
                return True

        return False

    def get_card_in_fold_cards(self, card_number) -> Card:
        legal = self.card_in_fold(card_number)
        if not legal:
            logging.error(f"card {card_number} is not in player {self.player_id} fold cards")
        for card in self.fold_cards:
            if card.number == card_number:
                return card

        return None

    def get_gemstone_number(self, gemstone_str) -> int:
        # replace getattr just for search purpose
        return getattr(self, gemstone_str)
