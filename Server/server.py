import socket
import threading

from Server.constants import HEADER_LENGTH
from Server.game_room import GameRoom
from Server.message_helper import unpackHeader,unpackBody
from Server.api_id import API_ID


def handleClient(current_game_room:GameRoom,client_sock:socket.socket, addr):
    # TODO: if client_sock has been connect
    while True:
        header_data = client_sock.recv(HEADER_LENGTH)
        header = unpackHeader(header_data)
        msg_body_len = header.msg_len - HEADER_LENGTH
        body = None
        if msg_body_len > 0:
            body_data = client_sock.recv(msg_body_len)
            body = unpackBody(body_data)

        # TODO: refactor this
        if header.api_id == API_ID.INIT:
            current_game_room.addPlayer(client_sock)

        if header.api_id == API_ID.PLAYER_READY:
            current_game_room.playerReady(header, body)

        if header.api_id == API_ID.PLAYER_OPERATION:
            current_game_room.doPlayerOperation(header, body)


def startListen(current_game_room:GameRoom):
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # TODO: read from config file
    sock.bind(('127.0.0.1', 13204))
    sock.listen()
    while True:
        client_sock, addr = sock.accept()
        t = threading.Thread(target=handleClient, args=(current_game_room,
                                                        client_sock, addr))
        t.start()


def start():
    # TODO: better way to get GameRoom
    current_game_room = GameRoom()
    startListen(current_game_room)
