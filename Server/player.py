import json
import socket

from Server.gemstone import Gemstone
from Server.card import Card

class Player:
    def __init__(self, sock:socket.socket, player_id:int):
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

        return json.dumps(tmp)

    def sendMsg(self, msg):
        self.sock.send(msg)

    def setReady(self):
        self.ready = True

    def addCard(self, card:Card, operation_info):
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
                        self.chips[gems_type] -= item["gems_number"]
                setattr(self, card.gem_type, getattr(self, card.gem_type)+1)

    def addFoldCard(self, card:Card):
        self.fold_cards.append(card)
