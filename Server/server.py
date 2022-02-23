import socket
import threading
import logging
from time import sleep

from Server.constants import HEADER_LENGTH, SERVER_ADDRESS
from Server.game_room import GameRoom
from Server.message_helper import unpackHeader, unpackBody
from Server.api_id import API_ID

ALL_CLIENT_SOCK = []
ALL_CLIENT_SOCK_LOCK = threading.Lock()
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


def handleClient(current_game_room: GameRoom, client_sock: socket.socket,
                 addr):
    logging.info("Start handling new socket, addr is {}".format(addr))
    while True:
        try:
            header_data = client_sock.recv(HEADER_LENGTH)
        except ConnectionResetError:
            logging.info("socket {} reset connection".format(client_sock))
            client_sock.close()
            return
        if len(header_data) < HEADER_LENGTH:
            logging.error(
                "header data length less than {}".format(HEADER_LENGTH))
            with ALL_CLIENT_SOCK_LOCK:
                ALL_CLIENT_SOCK.remove(client_sock)
            client_sock.close()
            return
        header = unpackHeader(header_data)
        msg_body_len = header.msg_len - HEADER_LENGTH
        logging.debug("Receive new msg, api id {}, msg length {}".format(
            header.api_id, header.msg_len))
        body = None
        if msg_body_len > 0:
            body_data = client_sock.recv(msg_body_len)
            body = unpackBody(body_data)
            logging.debug("body is {}".format(body_data.decode()))

        # TODO: check player_id match socket?
        # TODO: what if socket change?
        if header.api_id == API_ID.INIT:
            current_game_room.addPlayer(client_sock)

        if header.api_id == API_ID.PLAYER_READY:
            current_game_room.playerReady(header)

        if header.api_id == API_ID.PLAYER_OPERATION:
            current_game_room.doPlayerOperation(header, body)

        if header.api_id == API_ID.PLAYER_GET_NOBLE:
            current_game_room.doPlayerGetNoble(header, body)

        logging.debug("current game room info is:")
        logging.debug(str(current_game_room))


def startListen():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(SERVER_ADDRESS)
    sock.listen()
    logging.info("Server address is {}".format(SERVER_ADDRESS))
    while True:
        client_sock, addr = sock.accept()
        with ALL_CLIENT_SOCK_LOCK:
            ALL_CLIENT_SOCK.append(client_sock)
        logging.info("New socket connect")
        current_game_room = getCurrentGameroom()
        t = threading.Thread(target=handleClient,
                             args=(current_game_room, client_sock, addr))
        t.setDaemon(False)
        t.start()
        with FIRST_TIME_LOCK:
            global FIRST_TIME
            FIRST_TIME = False


def init_log():
    logging.basicConfig(filename="./server.log", level=logging.DEBUG)
    logging.info("___________________________________________________________")
    logging.info("Server starting")
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
                with ALL_CLIENT_SOCK_LOCK:
                    if len(ALL_CLIENT_SOCK) == 0:
                        return
