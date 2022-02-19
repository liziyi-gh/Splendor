import csv
import json
from collections import namedtuple

csv_file_path = "./card_configuration.csv"
json_file_path = "./card_configuration.json"

json_scheme = {
    "number": 0,
    "card_type": "",
    "level": 0,
    "points": 0,
    "gem_type": "",
    "chips": {
        "ruby": 0,
        "diamond": 0,
        "sapphire": 0,
        "emerald": 0,
        "obsidian": 0
    }
}

Card = namedtuple("Card", [
    "number", "type", "level", "points", "gem_type", "ruby", "diamond",
    "sapphire", "emerald", "obsidian"
])

with open(csv_file_path) as f:
    reader = csv.reader(f)
    raw_cards_data = list(reader)[1:]

print(raw_cards_data)

with open(json_file_path, 'w') as f:
    f.write("[")
    for i in range(len(raw_cards_data)):
        raw_card = raw_cards_data[i]
        card = Card(raw_card[0], raw_card[1], raw_card[2], raw_card[3],
                    raw_card[4], raw_card[5], raw_card[6], raw_card[7],
                    raw_card[8], raw_card[9])
        json_scheme["number"] = int(card.number)
        json_scheme["card_type"] = card.type
        json_scheme["level"] = int(card.level)
        json_scheme["points"] = int(card.points)
        json_scheme["gem_type"] = card.gem_type
        json_scheme["chips"]["ruby"] = int(card.ruby)
        json_scheme["chips"]["diamond"] = int(card.diamond)
        json_scheme["chips"]["sapphire"] = int(card.sapphire)
        json_scheme["chips"]["emerald"] = int(card.emerald)
        json_scheme["chips"]["obsidian"] = int(card.obsidian)
        str_json_card = json.dumps(json_scheme, indent=2)
        print(str_json_card)
        f.write(str_json_card)
        if i != len(raw_cards_data) - 1:
            f.write(",")
    f.write("]")
