from gemstone import Gemstone
from player import Player

class GameRoom:
    def __init__(self) -> None:
        self.players = []
        self.chips = {
            Gemstone.GOLDEN: 0,
            Gemstone.RUBY: 0,
            Gemstone.DIAMOND: 0,
            Gemstone.SAPPHIRE: 0,
            Gemstone.EMERALD: 0,
            Gemstone.OBSIDIAN: 0,
        }

    # TODO add lock
    def addPlayer(self, sock):
        new_player = Player(sock)
        self.players.append(new_player)

    # TODO add lock
    def boardcastMsg(self, msg):
        for player in self.players:
            player.sendMsg(msg)
