import json
import logging
import random
from typing import List

from Server.card import Card
from Server.constants import CARD_CONFIGUATION_FILE_PATH


class CardBoard():
    WIDTH_OF_CARD_IN_BOARD = 4

    def __init__(self) -> None:
        # TODO: read json only once in init server
        self.card_repo = []
        with open(CARD_CONFIGUATION_FILE_PATH) as f:
            card_configration = json.load(f)
        for item in card_configration:
            card = Card.createFromJson(item)
            self.card_repo.append(card)
        random.shuffle(self.card_repo)
        self.used_cards = []
        self.noble_cards = [self.get_next_card_in_repo(0)]
        self.level_one_cards = []
        self.level_two_cards = []
        self.level_three_cards = []
        for _ in range(self.WIDTH_OF_CARD_IN_BOARD):
            self.add_new_card_to_board(1)
            self.add_new_card_to_board(2)
            self.add_new_card_to_board(3)

    def get_next_card_in_repo(self, card_level) -> Card:
        for item in self.card_repo:
            if item.level == card_level:
                self.used_cards.append(item)
                self.card_repo.remove(item)
                return item

        logging.debug(
            "Can not find next level{} card in repo".format(card_level))

        return Card(None)

    def add_player(self):
        new_card = self.get_next_card_in_repo(0)
        self.noble_cards.append(new_card)

    def get_card_by_number(self, card_number: int) -> Card:
        for item in self.level_one_cards:
            if item.number == card_number:
                return item

        target_card = [
            item for item in self.level_one_cards + self.level_two_cards +
            self.level_three_cards + self.noble_cards
            if item.number == card_number
        ]

        if target_card != []:
            return target_card[0]
        else:
            logging.error("Could not find card by number {}".format(card_number))
            return None

    def add_new_card_to_board(self, card_level, original_card=None) -> int:

        def addCardHelper(new_card, card_list: List[Card], original_card=None):
            if original_card is not None:
                position = card_list.index(original_card)
                card_list[position] = new_card
            else:
                card_list.append(new_card)

        new_card = self.get_next_card_in_repo(card_level)

        if card_level == 1:
            addCardHelper(new_card, self.level_one_cards, original_card)

        if card_level == 2:
            addCardHelper(new_card, self.level_two_cards, original_card)

        if card_level == 3:
            addCardHelper(new_card, self.level_three_cards, original_card)

        return new_card.number

    def remove_card_by_number_then_add_new_card(self, card_number: int) -> int:
        card = self.get_card_by_number(card_number)
        if card in self.noble_cards:
            self.noble_cards.remove(card)
            self.used_cards.append(card)

            return -1
        else:
            if card_number in [10001, 10002, 10003]:
                return -1
            new_card_number = self.add_new_card_to_board(card.level, card)
            return new_card_number
