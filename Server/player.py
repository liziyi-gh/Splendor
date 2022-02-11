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

    def addCard(self, card:Card):
        pass
