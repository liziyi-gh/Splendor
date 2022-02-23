import json
import struct
from typing import List

from Server.api_id import API_ID
from Server.card_board import CardBoard
from Server.card import Card
from Server.constants import HEADER_FORMAT, HEADER_LENGTH, Header


def unpack_header(header_data) -> Header:
    api_id, player_id, msg_len, reserve = struct.unpack(
        HEADER_FORMAT, header_data)
    return Header(api_id, player_id, msg_len, reserve)


def unpack_body(body_data):
    return json.loads(body_data.decode())


def pack_header(api_id, player_id, body_len=0):
    return struct.pack(HEADER_FORMAT, api_id, player_id,
                       HEADER_LENGTH + body_len, 0)


def pack_universial(body, api_id: int, player_id=0):
    body_data = json.dumps(body).encode()
    header_data = pack_header(api_id, player_id, len(body_data))

    return header_data + body_data


def pack_init_resp(new_player_id, allocated_player_id):
    tmp_dict = {
        "allocated_player_id":
        new_player_id,
        "other_player_id":
        [id for id in allocated_player_id if id != new_player_id],
    }
    return pack_universial(tmp_dict, API_ID.INIT_RESP, new_player_id)


def pack_player_operation_invalid(player_id):
    header_data = pack_header(API_ID.PLAYER_OPERATION_INVALID, player_id)

    return header_data


def pack_player_ready(player_id):
    header_data = pack_header(API_ID.PLAYER_READY, player_id)

    return header_data


def pack_game_start(players_number, players_sequence, card_board: CardBoard):
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

    return pack_universial(tmp_dict, API_ID.GAME_START)


def pack_new_turn(player_id: int):
    tmp_dict = {"new_turn_player": player_id}

    return pack_universial(tmp_dict, API_ID.NEW_TURN)


def pack_new_player(player_id: int):
    header_data = pack_header(API_ID.NEW_PLAYER, player_id)

    return header_data


def pack_player_operation(body):
    return pack_universial(body, API_ID.PLAYER_OPERATION)


def pack_player_get_noble(player_id: int, card: Card):
    tmp_dict = {
        "player_id": player_id,
        "noble_number": [card.number],
    }

    return pack_universial(tmp_dict, API_ID.PLAYER_GET_NOBLE, player_id)


def pack_ask_player_get_noble(player_id: int, cards: List[Card]):
    tmp_dict = {
        "player_id": player_id,
        "noble_number": [card.number for card in cards],
    }

    return pack_universial(tmp_dict, API_ID.PLAYER_GET_NOBLE, player_id)


def pack_new_card(player_id: int, card_number: int):
    tmp_dict = {
        "card_number": card_number,
    }

    return pack_universial(tmp_dict, API_ID.NEW_CARD, player_id)


def pack_discard_gems(player_id: int, number_to_discard: int):
    tmp_dict = {"number_to_discard": number_to_discard}

    return pack_universial(tmp_dict, API_ID.DISCARD_GEMS, player_id)


def pack_winner(player_id: int):
    return pack_header(API_ID.WINNER, player_id)
