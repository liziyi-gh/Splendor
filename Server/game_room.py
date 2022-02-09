import socket
import threading

from Server.gemstone import Gemstone
from Server.message_helper import Header
from Server.player import Player

def thread_safe(function):

    def async_wrapper(self, *args, **kwargs):
        with self.lock:
            ret = function(self, *args, **kwargs)
            return ret

    return async_wrapper


class GameRoom:
    def __init__(self) -> None:
        self.lock = threading.Lock()
        self.players = []
        self.allocated_id = []
        self.allow_player_id = (1, 2, 3, 4)
        self.chips = {
            Gemstone.GOLDEN: 0,
            Gemstone.RUBY: 0,
            Gemstone.DIAMOND: 0,
            Gemstone.SAPPHIRE: 0,
            Gemstone.EMERALD: 0,
            Gemstone.OBSIDIAN: 0,
        }

    def findPlayerByID(self, player_id)->Player | None:
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
        # TODO:
        # new_player.sendMsg(msg)

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

    @thread_safe
    def doPlayerOperation(self, header:Header, body):
        pass
