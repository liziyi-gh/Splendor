import json
import struct
from collections import namedtuple

from Server.constants import HEADER_FORMAT

Header = namedtuple("Header", ["api_id", "player_id", "msg_len", "reserve"])

def unpackHeader(header_data)-> Header:
    api_id, player_id, msg_len, reserve = struct.unpack(HEADER_FORMAT,
                                                        header_data.decode())
    return Header(api_id, player_id, msg_len, reserve)


def unpackBody(body_data):
    return json.loads(body_data.decode())
