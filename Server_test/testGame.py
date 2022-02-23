import struct
import pytest

from Server import game_room, message_helper
from Server.api_id import API_ID
from Server.card_board import CardBoard
from Server.card import Card
from Server.constants import HEADER_FORMAT, HEADER_LENGTH, Header

def test_packHeader1():
    # Given
    player_id = 1
    api_id = 1
    body_len = 41
    reserve = 0
    expected_header = struct.pack(HEADER_FORMAT, api_id, player_id, HEADER_LENGTH+body_len, reserve)

    # When
    header = message_helper.pack_header(player_id, api_id, body_len)

    # Then
    assert(header == expected_header)


def test_packHeader2():
    # Given
    player_id = -23
    api_id = 1
    body_len = 41

    # When
    with pytest.raises(struct.error):
        _ = message_helper.pack_header(player_id, api_id, body_len)

    # Then



def test_unpackHeader():
    # Given
    player_id = 1
    api_id = 434
    body_len = 41
    reserve = 0
    header_data = struct.pack(HEADER_FORMAT, api_id, player_id, HEADER_LENGTH+body_len, reserve)

    # When
    header = message_helper.unpack_header(header_data)

    # Then
    assert(isinstance(header, Header))
    assert(header.player_id == player_id)
    assert(header.api_id == api_id)
    assert(header.msg_len == HEADER_LENGTH+body_len)
    assert(header.reserve == reserve)


def test_GameRoom_init():
    # Given
    room = game_room.GameRoom()
