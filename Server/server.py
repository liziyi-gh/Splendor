import socket
import logging
import selectors

from Server.constants import SERVER_ADDRESS
from Server.global_vars import sel
from Server.game_room import get_current_gameroom


def handle_accept(server_sock, mask):
    client_sock, _ = server_sock.accept()
    client_sock.setblocking(False)
    logging.info("New socket connect")
    current_game_room = get_current_gameroom()
    sel.register(client_sock, selectors.EVENT_READ, current_game_room.handle_client)


def start_listen():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(SERVER_ADDRESS)
    sock.setblocking(False)
    sock.listen(100)
    logging.info("Server address is {}".format(SERVER_ADDRESS))
    sel.register(sock, selectors.EVENT_READ, handle_accept)



def init_log():
    logging.basicConfig(filename="./server.log", level=logging.DEBUG)
    logging.info("___________________________________________________________")
    logging.info("                                                           ")
    logging.info("                    Server starting                        ")
    logging.info("                                                           ")
    logging.info("___________________________________________________________")


def start():
    init_log()
    start_listen()
    while True:
        events = sel.select()
        for key, mask in events:
            callback = key.data
            callback(key.fileobj, mask)
