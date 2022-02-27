import socket
from typing import Tuple

from Server import game_room, message_helper
from Server.api_id import API_ID
from Server.card_board import CardBoard
from Server.card import Card
from Server.constants import HEADER_FORMAT, HEADER_LENGTH, SERVER_ADDRESS, Header
from Server.test import client_message_helper


class Client():

    def __init__(self):
        self.sock = socket.socket()
        self.player_id = -1

    def send_msg(self, msg):
        self.sock.send(msg)

    def recv_msg(self) -> Tuple[Header | None, str | None]:
        header_data = self.sock.recv(HEADER_LENGTH)

        if len(header_data) < HEADER_LENGTH:
            return None, None

        header = message_helper.unpack_header(header_data)
        msg_body_len = header.msg_len - HEADER_LENGTH

        body = None
        if msg_body_len > 0:
            body_data = self.sock.recv(msg_body_len)
            body = message_helper.unpack_body(body_data)

        return header, body

    def init_with_server(self) -> Tuple[Header | None, str | None]:
        self.sock.connect(SERVER_ADDRESS)
        init_msg = client_message_helper.pack_init()
        self.send_msg(init_msg)
        init_resp_header, init_resp_body = self.recv_msg()

        return init_resp_header, init_resp_body

    def __del__(self):
        self.sock.close()
