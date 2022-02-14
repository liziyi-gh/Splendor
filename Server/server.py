import socket
import threading
import logging

from Server.constants import HEADER_LENGTH, SERVER_ADDRESS
from Server.game_room import GameRoom
from Server.message_helper import unpackHeader, unpackBody
from Server.api_id import API_ID


def handleClient(current_game_room: GameRoom, client_sock: socket.socket,
                 addr):
    logging.info("Start handling New socket, addr is {}".format(addr))
    while True:
        header_data = client_sock.recv(HEADER_LENGTH)
        header = unpackHeader(header_data)
        msg_body_len = header.msg_len - HEADER_LENGTH
        body = None
        if msg_body_len > 0:
            body_data = client_sock.recv(msg_body_len)
            body = unpackBody(body_data)

        # TODO: check player_id match socket?
        # TODO: what if socket change?
        if header.api_id == API_ID.INIT:
            current_game_room.addPlayer(client_sock)

        if header.api_id == API_ID.PLAYER_READY:
            current_game_room.playerReady(header, body)

        if header.api_id == API_ID.PLAYER_OPERATION:
            current_game_room.doPlayerOperation(header, body)

        if header.api_id == API_ID.PLAYER_GET_NOBLE:
            current_game_room.doPlayerGetNoble(header, body)


def startListen(current_game_room: GameRoom):
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(SERVER_ADDRESS)
    sock.listen()
    while True:
        client_sock, addr = sock.accept()
        logging.info("New socket connect")
        t = threading.Thread(target=handleClient,
                             args=(current_game_room, client_sock, addr))
        t.start()


def init_log():
    logging.basicConfig(filename="./server.log",
                        encoding='utf-8',
                        level=logging.DEBUG)
    logging.info("___________________________________________________________")
    logging.info("Server starting")
    logging.info("___________________________________________________________")


def start():
    init_log()
    current_game_room = GameRoom()
    startListen(current_game_room)
