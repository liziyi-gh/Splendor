using System;
using System.Collections.Generic;
using Gems;

namespace MsgStruct
{
    public class Msgs
    {
        public uint api_id = 0;
        public ulong player_id = 0L,
                     msg_len = 0L,
                     reserve = 0L;
        public int card_level = 0,
                   card_id = 0;
        public string? operation_type;
        public List<ulong> other_player_id = new List<ulong>();
        public List<int> nobles_id = new List<int>();
        public Dictionary<string, int> gems = new Dictionary<string, int>
        {
            { GEM.DIAMOND, 0 },
            { GEM.EMERALD, 0 },
            { GEM.OBSIDIAN, 0 },
            { GEM.SAPPHIRE, 0 },
            { GEM.RUBY, 0 },
            { GEM.GOLDEN, 0 }
        };
    }
}
