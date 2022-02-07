import socket

def startListen():
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    sock.bind(('127.0.0.1', 13204))
    sock.listen()
    while True:
        sock_client, addr = sock.accept()

def start():
    print("hello_world")
