class Player:
    def __init__(self):
        self.player_id = 0
        self.points = 0
        self.cards = []
        self.fold_cards = []

        self.ruby = 0
        self.diamond = 0
        self.sapphire = 0
        self.emerald = 0
        self.obsidian = 0

        self.chips = {
            "golden": 0,
            "ruby": 0,
            "diamond": 0,
            "sapphire": 0,
            "emerald": 0,
            "obsidian": 0,
        }
