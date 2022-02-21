from collections import namedtuple

SERVER_ADDRESS = ('192.168.50.144', 13204)
HEADER_LENGTH = 28
HEADER_FORMAT = "!L3Q"
CARD_CONFIGUATION_FILE_PATH = "card_configuration.json"

Header = namedtuple("Header", ["api_id", "player_id", "msg_len", "reserve"])
