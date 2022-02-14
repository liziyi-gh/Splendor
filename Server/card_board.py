import json
import logging
import random

from Server.card import Card, CardType
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
        self.nobles_info = [self.nextCardInRepo(0)]
        self.levelOneCards_info = []
        self.levelTwoCards_info = []
        self.levelThreeCards_info = []
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
        self.nobles_info.append(new_card)

    def getCardByNumber(self, card_number: int) -> Card:
        # TODO: more pythonic
        for item in self.levelOneCards_info:
            if item.number == card_number:
                return item

        target_card = [
            item
            for item in self.levelOneCards_info + self.levelTwoCards_info +
            self.levelThreeCards_info + self.nobles_info
            if item.number == card_number
        ]

        if target_card:
            return target_card[0]
        else:
            return None

    def addNewCardToBoard(self, card_level, original_card=None, position=None):
        card = self.nextCardInRepo(card_level)

        if card_level == 1:
            if position is not None:
                position = self.levelOneCards_info.index(original_card)
                self.levelOneCards_info[position] = card
            else:
                self.levelOneCards_info.append(card)

        if card_level == 2:
            if position is not None:
                position = self.levelTwoCards_info.index(original_card)
                self.levelTwoCards_info[position] = card
            else:
                self.levelTwoCards_info.append(card)

        if card_level == 3:
            if position is not None:
                position = self.levelThreeCards_info.index(original_card)
                self.levelThreeCards_info[position] = card
            else:
                self.levelThreeCards_info.append(card)

    def removeCardByNumberThenAddNewCard(self, card_number: int):
        card = self.getCardByNumber(card_number)
        if card in self.nobles_info:
            self.nobles_info.remove(card)
        else:
            self.addNewCardToBoard(card.level, card)

    def checkAvailbaleNobleCard(self, player: Player) -> list[Card]:
        cards = []
        for item in self.nobles_info:
            if player.checkAvailbaleNobleCard(item):
                cards.append(item)

        return cards
