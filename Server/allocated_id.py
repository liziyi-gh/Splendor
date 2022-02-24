import threading

from Server.client_socket import ALL_CLIENT_SOCK_LOCK
from Server.constants import MAX_PLAYER_ID

ALLOCATING_LOCK = threading.Lock()
ALLOCATED_ID = []

def new_player_id() -> int:
    with ALL_CLIENT_SOCK_LOCK:
        tmp = (i for i in range(MAX_PLAYER_ID) if i not in ALLOCATED_ID)
        for i in tmp:
            ALLOCATED_ID.append(i)
            return i
