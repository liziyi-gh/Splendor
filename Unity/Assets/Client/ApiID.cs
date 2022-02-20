﻿using System;

namespace ApiID
{
    public class API_ID
    {
        public const uint INIT = 1,
                          INIT_RESP = 2,
                          PLAYER_READY = 3,
                          GAME_START = 4,
                          NEW_TURN = 5,
                          PLAYER_OPERATION = 6,
                          PLAYER_OPERATION_INVALID = 7,
                          NEW_PLAYER = 8,
                          PLAYER_GET_NOBLE = 9,
                          NEW_CARD = 10,
                          DISCARD_GEMS = 11;
    }
}
