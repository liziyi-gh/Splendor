import json
import struct

from collections import namedtuple
from Server.api_id import API_ID
from Server.card_board import CardBoard
from Server.card import Card
from Server.constants import HEADER_FORMAT, HEADER_LENGTH

Header = namedtuple("Header", ["api_id", "player_id", "msg_len", "reserve"])

# TODO: refactor this

def unpackHeader(header_data)-> Header:
    api_id, player_id, msg_len, reserve = struct.unpack(HEADER_FORMAT, header_data)
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


def packPlayerReady(player_id):
    header_data = packHeader(API_ID.PLAYER_READY, player_id)

    return header_data


def packGameStart(players_number, players_sequence,
                  card_board:CardBoard):
    tmp_dict = {
        "players_number", players_number,
        "players_sequence", players_sequence,
        "nobels_info", card_board.nobels_info,
        "levelOneCards_info", card_board.levelOneCards_info,
        "levelTwoCards_info", card_board.levelTwoCards_info,
        "levelThreeCards_info", card_board.levelThreeCards_info,
    }
    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.GAME_START, 0, len(body_data))

    return header_data + body_data


def packNewTurn(player_id:int):
    tmp_dict = {"new_turn_player": player_id}
    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.NEW_TURN, 0, len(body_data))

    return header_data + body_data


def packNewPlayer(player_id:int):
    header_data = packHeader(API_ID.NEW_PLAYER, player_id)

    return header_data


def packPlayerOperation(body):
    body_data = json.dumps(body).encode()
    header_data = packHeader(API_ID.PLAYER_OPERATION, 0, len(body_data))

    return header_data + body_data


def packPlayerGetNoble(player_id:int, card:Card):
    tmp_dict = {
        "player_id": player_id,
        "noble_number": card.number,
    }

    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.NEW_TURN, player_id, len(body_data))

    return header_data + body_data


def packAskPlayerGetNoble(player_id:int, cards:list[Card]):
    tmp_dict = {
        "player_id": player_id,
        "noble_number": [card.number for card in cards],
    }

    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.NEW_TURN, player_id, len(body_data))

    return header_data + body_data
