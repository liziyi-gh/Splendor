import threading

from Server.constants import MAX_PLAYER_ID

ALLOCATING_LOCK = threading.Lock()
ALLOCATED_ID = []

def new_player_id() -> int:
    with ALLOCATING_LOCK:
        tmp = (i for i in range(MAX_PLAYER_ID) if i not in ALLOCATED_ID)
        for i in tmp:
            ALLOCATED_ID.append(i)
            return i


def remove_player_id(id: int) -> bool:
    with ALLOCATING_LOCK:
        try:
            ALLOCATED_ID.remove(id)
            return True
        except ValueError:
            return False
