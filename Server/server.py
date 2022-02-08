import socket
import threading
import struct
import json
from collections import namedtuple
from constants import HEADER_LENGTH, HEADER_FORMAT
from game_room import GameRoom


class API_ID:
    INIT = 1
    INIT_RESP = 2
    PLAYER_READY = 3
    GAME_START = 4
    NEW_TURN = 5
    PLAYER_OPERATION = 6


Header = namedtuple("Header", ["api_id", "player_id", "msg_len", "reserve"])

def unpackHeader(header_data)-> Header:
    api_id, player_id, msg_len, reserve = struct.unpack(HEADER_FORMAT,
                                                        header_data.decode())
    return Header(api_id, player_id, msg_len, reserve)


def unpackBody(body_data):
    return json.loads(body_data.decode())


# TODO: better way to get GameRoom
current_game_room = GameRoom()


def handleClient(client_sock:socket.socket, addr):
    # TODO: if client_sock has been connect
    header_data = client_sock.recv(HEADER_LENGTH)
    header = unpackHeader(header_data)
    msg_body_len = header.msg_len - HEADER_LENGTH
    if msg_body_len > 0:
        body_data = client_sock.recv(msg_body_len)
        body = unpackBody(body_data)

    # TODO: refactor this
    if header.api_id == API_ID.INIT:
        current_game_room.addPlayer(client_sock)




def startListen():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    # TODO: read from config file
    sock.bind(('127.0.0.1', 13204))
    sock.listen()
    while True:
        client_sock, addr = sock.accept()
        t = threading.Thread(target=handleClient, args=(client_sock, addr))
        t.start()

def start():
    startListen()
