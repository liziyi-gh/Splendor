import json
import random

from Server.card import Card, CardType
from Server.constants import CARD_CONFIGUATION_FILE_PATH


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

        self.nobels_info = [self.nextCardInRepo(0)]
        self.levelOneCards_info = []
        self.levelTwoCards_info = []
        self.levelThreeCards_info = []
        for i in range(self.WIDTH_OF_CARD_IN_BOARD):
            self.addNewCardToBoard(1, position=i)
            self.addNewCardToBoard(2, position=i)
            self.addNewCardToBoard(3, position=i)

    def nextCardInRepo(self, card_level)->Card:
        for item in self.card_repo:
            if item.level==card_level:
                self.card_repo.remove(item)
                return item

    def addPlayer(self):
        new_card = self.nextCardInRepo(0)
        self.nobels_info.append(new_card)

    def getCardByNumber(self, card_number: int) -> Card:
        # TODO: more pythonic
        for item in self.levelOneCards_info:
            if item.number == card_number:
                return item

        target_card = [
            item
            for item in self.levelOneCards_info + self.levelTwoCards_info +
            self.levelThreeCards_info + self.nobels_info
            if item.number == card_number
        ]

        if target_card:
            return target_card[0]
        else:
            return None

    def addNewCardToBoard(self, card_level, original_card=None, position=None):
        card = self.nextCardInRepo(card_level)

        if card_level == 1:
            if not position:
                position = self.levelOneCards_info.index(original_card)
            self.levelOneCards_info[position] = card

        if card_level == 2:
            if not position:
                position = self.levelTwoCards_info.index(original_card)
            self.levelTwoCards_info[position] = card

        if card_level == 3:
            if not position:
                position = self.levelThreeCards_info.index(original_card)
            self.levelThreeCards_info[position] = card

    def removeCardByNumberThenAddNewCard(self, card_number: int):
        card = self.getCardByNumber(card_number)
        if card in self.nobels_info:
            self.nobels_info.remove(card)
        else:
            self.addNewCardToBoard(card.level, card)
