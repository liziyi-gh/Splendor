import threading

ALL_CLIENT_SOCK = []
ALL_CLIENT_SOCK_LOCK = threading.Lock()

def append(client_sock):
    global ALL_CLIENT_SOCK
    with ALL_CLIENT_SOCK_LOCK:
        ALL_CLIENT_SOCK.append(client_sock)

def remove(client_sock):
    global ALL_CLIENT_SOCK
    with ALL_CLIENT_SOCK_LOCK:
        ALL_CLIENT_SOCK.remove(client_sock)

def length():
    global ALL_CLIENT_SOCK
    with ALL_CLIENT_SOCK_LOCK:
        return len(ALL_CLIENT_SOCK)
