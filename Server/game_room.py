import threading
from functools import wraps

from Server.gemstone import Gemstone
from Server.player import Player

def thread_safe(function):

    @wraps
    def async_wrapper(self, *args, **kwargs):
        with self.lock:
            ret = function(self, *args, **kwargs)
            return ret

    return async_wrapper


class GameRoom:
    def __init__(self) -> None:
        self.lock = threading.Lock()
        self.players = []
        self.chips = {
            Gemstone.GOLDEN: 0,
            Gemstone.RUBY: 0,
            Gemstone.DIAMOND: 0,
            Gemstone.SAPPHIRE: 0,
            Gemstone.EMERALD: 0,
            Gemstone.OBSIDIAN: 0,
        }

    @thread_safe
    def addPlayer(self, sock):
        new_player = Player(sock)
        self.players.append(new_player)
        # new_player.sendMsg(msg)

    @thread_safe
    def boardcastMsg(self, msg):
        # TODO: may need Multithreading
        for player in self.players:
            player.sendMsg(msg)
