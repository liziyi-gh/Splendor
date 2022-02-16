from collections import namedtuple

SERVER_ADDRESS = ('127.0.0.1', 13204)
HEADER_LENGTH = 28
HEADER_FORMAT = "!L3Q"
CARD_CONFIGUATION_FILE_PATH = "card_configuration.json"

Header = namedtuple("Header", ["api_id", "player_id", "msg_len", "reserve"])
