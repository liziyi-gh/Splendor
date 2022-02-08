import json
from gemstone import Gemstone

class Card:
    @classmethod
    def createFromJson(cls, j_str):
        j_dict = json.loads(j_str)

        return cls(j_dict["level"], j_dict["points"], j_dict["gem_type"],
                   j_dict["chips"])

    def __init__(self, level=0, points=0, gem_type=Gemstone.NIL, dict_chips=None):
        self.level = level
        self.points = points
        self.gem_type = gem_type

        # This indicate how many chips need to cash this card
        self.chips = dict_chips if dict_chips else {
            Gemstone.GOLDEN: 0,
            Gemstone.RUBY: 0,
            Gemstone.DIAMOND: 0,
            Gemstone.SAPPHIRE: 0,
            Gemstone.EMERALD: 0,
            Gemstone.OBSIDIAN: 0,
        }

    def __str__(self):
        return json.dumps(self.__dict__)
