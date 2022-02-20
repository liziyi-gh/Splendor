import json
import logging
import socket
import struct

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

    def sendMsg(self, msg):
        self.sock.send(msg)
        header_data = struct.unpack(HEADER_FORMAT, msg[0:HEADER_LENGTH])
        logging.debug("Send msg to player {}, api {}, body is".format(
            self.player_id, header_data[0]))
        if header_data[2] > HEADER_LENGTH:
            body_data = msg[HEADER_LENGTH:]
            body = json.loads(body_data.decode())
            logging.debug(body)

    def setReady(self):
        logging.info("Player {} ready".format(self.player_id))
        self.ready = True

    def addCard(self, card: Card, operation_info=None):
        if card.level == 0:
            # Noble card
            self.points += card.points
        else:
            # Normal card
            if card in self.fold_cards:
                self.fold_cards.remove(card)
            if card.gem_type in Gemstone.__dict__.values():
                for item in operation_info:
                    if "gems_type" in item.keys():
                        gems_type = item["gems_type"]
                        gems_number = int(item["gems_number"])
                        player_gems_number = getattr(self, gems_type)
                        if player_gems_number < gems_number:
                            self.chips[gems_type] = self.chips[gems_type] - (
                                gems_number - player_gems_number)
                        self.chips[gems_type] -= gems_number

                setattr(self, card.gem_type, getattr(self, card.gem_type) + 1)

        self.cards.append(card)
        logging.info("Player {} got card {}".format(self.player_id,
                                                    card.number))

    def addFoldCard(self, card: Card):
        self.fold_cards.append(card)
        logging.info("Player {} fold card {}".format(self.player_id,
                                                     card.number))

    def checkAvailbaleNobleCard(self, card: Card) -> bool:
        for gemstone in card.chips.keys():
            gem_numbers = getattr(self, gemstone)
            if gem_numbers < card.chips[gemstone]:
                return False

        return True

    def getAllChipsNumber(self) -> int:
        ans = 0
        for _, v in self.chips.items():
            ans += v

        return ans
