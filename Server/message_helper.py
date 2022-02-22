import json
import struct
from typing import List

from Server.api_id import API_ID
from Server.card_board import CardBoard
from Server.card import Card
from Server.constants import HEADER_FORMAT, HEADER_LENGTH, Header

# TODO: refactor this


def unpackHeader(header_data) -> Header:
    api_id, player_id, msg_len, reserve = struct.unpack(
        HEADER_FORMAT, header_data)
    return Header(api_id, player_id, msg_len, reserve)


def unpackBody(body_data):
    return json.loads(body_data.decode())


def packHeader(api_id, player_id, body_len=0):
    return struct.pack(HEADER_FORMAT, api_id, player_id,
                       HEADER_LENGTH + body_len, 0)


def packInitResp(new_player_id, allocated_player_id):
    tmp_dict = {
        "allocated_player_id":
        new_player_id,
        "other_player_id":
        [id for id in allocated_player_id if id != new_player_id],
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


def packGameStart(players_number, players_sequence, card_board: CardBoard):
    tmp_dict = {
        "players_number":
        players_number,
        "players_sequence":
        players_sequence,
        "nobles_info": [card.number for card in card_board.noble_cards],
        "levelOneCards_info":
        [card.number for card in card_board.level_one_cards],
        "levelTwoCards_info":
        [card.number for card in card_board.level_two_cards],
        "levelThreeCards_info":
        [card.number for card in card_board.level_three_cards],
    }
    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.GAME_START, 0, len(body_data))

    return header_data + body_data


def packNewTurn(player_id: int):
    tmp_dict = {"new_turn_player": player_id}
    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.NEW_TURN, 0, len(body_data))

    return header_data + body_data


def packNewPlayer(player_id: int):
    header_data = packHeader(API_ID.NEW_PLAYER, player_id)

    return header_data


def packPlayerOperation(body):
    body_data = json.dumps(body).encode()
    header_data = packHeader(API_ID.PLAYER_OPERATION, 0, len(body_data))

    return header_data + body_data


def packPlayerGetNoble(player_id: int, card: Card):
    tmp_dict = {
        "player_id": player_id,
        "noble_number": [card.number],
    }

    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.PLAYER_GET_NOBLE, player_id, len(body_data))

    return header_data + body_data


def packAskPlayerGetNoble(player_id: int, cards: List[Card]):
    tmp_dict = {
        "player_id": player_id,
        "noble_number": [card.number for card in cards],
    }

    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.PLAYER_GET_NOBLE, player_id, len(body_data))

    return header_data + body_data


def packNewCard(player_id : int, card_number : int):
    tmp_dict = {
        "card_number": card_number,
    }
    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.NEW_CARD, player_id, len(body_data))

    return header_data + body_data


def packDiscardGems(player_id : int, number_to_discard : int):
    tmp_dict = {
        "number_to_discard": number_to_discard
    }
    body_data = json.dumps(tmp_dict).encode()
    header_data = packHeader(API_ID.DISCARD_GEMS, player_id, len(body_data))

    return header_data + body_data

def packUniversial(body, api_id: int):
    body_data = json.dumps(body).encode()
    header_data = packHeader(api_id, 0, len(body_data))

    return header_data + body_data
