import socket
import threading
import logging
from time import sleep

from Server.constants import SERVER_ADDRESS
from Server.game_room import GameRoom
from Server import client_socket


FIRST_TIME = True
FIRST_TIME_LOCK = threading.Lock()
GAME_ROOM_LIST = []


# FIXME may be thread-unsafe but can not use func_helper.thread_safe
def getCurrentGameroom() -> GameRoom:
    # FIXME: cause memory leak
    # gabage collection
    global GAME_ROOM_LIST
    if GAME_ROOM_LIST == []:
        new_game_room = GameRoom()
        GAME_ROOM_LIST.append(new_game_room)
        logging.info("Create new room!")
        return new_game_room
    else:
        tmp_room = GAME_ROOM_LIST[-1]
        if tmp_room.started or len(tmp_room.allocated_id) >= 4:
            new_game_room = GameRoom()
            GAME_ROOM_LIST.append(new_game_room)
            return new_game_room
        else:
            return tmp_room

def startListen():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(SERVER_ADDRESS)
    sock.listen()
    logging.info("Server address is {}".format(SERVER_ADDRESS))
    while True:
        client_sock, addr = sock.accept()
        client_socket.append(client_sock)
        logging.info("New socket connect")
        current_game_room = getCurrentGameroom()
        t = threading.Thread(target=current_game_room.handleClient,
                             args=(client_sock, addr))
        t.setDaemon(False)
        t.start()
        with FIRST_TIME_LOCK:
            global FIRST_TIME
            FIRST_TIME = False


def init_log():
    logging.basicConfig(filename="./server.log", level=logging.DEBUG)
    logging.info("___________________________________________________________")
    logging.info("                                                           ")
    logging.info("                    Server starting                        ")
    logging.info("                                                           ")
    logging.info("___________________________________________________________")


def start():
    init_log()
    t = threading.Thread(target=startListen)
    t.setDaemon(True)
    t.start()
    while True:
        sleep(1)
        with FIRST_TIME_LOCK:
            if not FIRST_TIME:
                if client_socket.length() == 0:
                    return
