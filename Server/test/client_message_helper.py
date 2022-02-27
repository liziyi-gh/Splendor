import struct
from Server.api_id import API_ID

from Server.constants import HEADER_FORMAT

def pack_init():
    return struct.pack(HEADER_FORMAT, API_ID.INIT, 0, 28, 0)
