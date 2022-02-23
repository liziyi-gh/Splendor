import json

from Server.gemstone import Gemstone


class CardType:
    NORMAL = "normal"
    NOBLE = "noble"


class Card:

    @classmethod
    def createFromJson(cls, j_dict):
        card_type = j_dict["card_type"]

        if card_type == CardType.NOBLE:
            return cls(card_type,
                       j_dict["number"],
                       points=j_dict["points"],
                       chips=j_dict["chips"])

        if card_type == CardType.NORMAL:
            return cls(card_type, j_dict["number"], j_dict["level"],
                       j_dict["points"], j_dict["gem_type"], j_dict["chips"])

    def __init__(self,
                 card_type,
                 number=0,
                 level=0,
                 points=0,
                 gem_type=Gemstone.NIL,
                 chips=None):
        self.card_type = card_type
        self.number = int(number)
        self.level = int(level)
        self.points = int(points)
        self.gem_type = gem_type

        # This indicate how many chips need to cash this card
        self.chips = chips if chips else {
            Gemstone.GOLDEN: 0,
            Gemstone.RUBY: 0,
            Gemstone.DIAMOND: 0,
            Gemstone.SAPPHIRE: 0,
            Gemstone.EMERALD: 0,
            Gemstone.OBSIDIAN: 0,
        }

    def __str__(self):
        return json.dumps(self.__dict__)
