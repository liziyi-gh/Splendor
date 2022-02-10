import json
import struct

from collections import namedtuple
from Server.api_id import API_ID

from Server.constants import HEADER_FORMAT, HEADER_LENGTH

Header = namedtuple("Header", ["api_id", "player_id", "msg_len", "reserve"])

def unpackHeader(header_data)-> Header:
    api_id, player_id, msg_len, reserve = struct.unpack(HEADER_FORMAT,
                                                        header_data.decode())
    return Header(api_id, player_id, msg_len, reserve)


def unpackBody(body_data):
    return json.loads(body_data.decode())


def packHeader(api_id, player_id, body_len=0):
    return struct.pack(HEADER_FORMAT, api_id, player_id, 0, HEADER_LENGTH+body_len)


def packInitResp(new_player_id, allocated_player_id):
    tmp_dict = {
        "allocated_player_id": new_player_id,
        "other_player_id": [id for id in allocated_player_id if id != new_player_id],
    }
    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.INIT_RESP, new_player_id, len(body_data))

    return header_data + body_data

def packPlayerOperationInvalid(player_id):
    header_data = packHeader(API_ID.PLAYER_OPERATION_INVALID, player_id)
    return header_data
