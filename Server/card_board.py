import json
import logging
import random

from Server.card import Card
from Server.constants import CARD_CONFIGUATION_FILE_PATH
from Server.player import Player


class CardBoard():
    WIDTH_OF_CARD_IN_BOARD = 4

    def __init__(self) -> None:
        self.card_repo = []
        with open(CARD_CONFIGUATION_FILE_PATH) as f:
            card_configration = json.load(f)
        for item in card_configration:
            card = Card.createFromJson(item)
            self.card_repo.append(card)
        random.shuffle(self.card_repo)
        self.used_cards = []
        self.noble_cards = [self.nextCardInRepo(0)]
        self.level_one_cards = []
        self.level_two_cards = []
        self.level_three_cards = []
        for i in range(self.WIDTH_OF_CARD_IN_BOARD):
            self.addNewCardToBoard(1)
            self.addNewCardToBoard(2)
            self.addNewCardToBoard(3)

    def nextCardInRepo(self, card_level) -> Card:
        for item in self.card_repo:
            if item.level == card_level:
                self.used_cards.append(item)
                self.card_repo.remove(item)
                return item

        logging.debug(
            "Can not find next level{} card in repo".format(card_level))

    def addPlayer(self):
        new_card = self.nextCardInRepo(0)
        self.noble_cards.append(new_card)

    def getCardByNumber(self, card_number: int) -> Card:
        # TODO: more pythonic
        for item in self.level_one_cards:
            if item.number == card_number:
                return item

        target_card = [
            item
            for item in self.level_one_cards + self.level_two_cards +
            self.level_three_cards + self.noble_cards
            if item.number == card_number
        ]

        if target_card != []:
            return target_card[0]
        else:
            logging.error("Could not find card by number {}".format(card_number))
            return None

    def addNewCardToBoard(self, card_level, original_card=None):
        card = self.nextCardInRepo(card_level)

        if card_level == 1:
            if original_card is not None:
                position = self.level_one_cards.index(original_card)
                self.level_one_cards[position] = card
            else:
                self.level_one_cards.append(card)

        if card_level == 2:
            if original_card is not None:
                position = self.level_two_cards.index(original_card)
                self.level_two_cards[position] = card
            else:
                self.level_two_cards.append(card)

        if card_level == 3:
            if original_card is not None:
                position = self.level_three_cards.index(original_card)
                self.level_three_cards[position] = card
            else:
                self.level_three_cards.append(card)

    def removeCardByNumberThenAddNewCard(self, card_number: int) -> int:
        card = self.getCardByNumber(card_number)
        if card in self.noble_cards:
            self.noble_cards.remove(card)
            self.used_cards.append(card)

            return -1
        else:
            self.addNewCardToBoard(card.level, card)
            return card.number

    def checkAvailbaleNobleCard(self, player: Player) -> list[Card]:
        cards = []
        for item in self.noble_cards:
            if player.checkAvailbaleNobleCard(item):
                cards.append(item)

        return cards
