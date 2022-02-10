using System;

namespace MsgStruct
{
    public struct Msgs
    {
        public int api_id = 0, player_id = 0, card_num = 0;
        public string? operation_type;
        public Dictionary<string, int> chips = new Dictionary<string, int> 
        { 
            { "ruby", 0 }, 
            { "diamond", 0 }, 
            { "sapphire", 0 }, 
            { "emerald", 0 }, 
            { "obsidian", 0 }, 
            { "golden", 0 } 
        };
    };
    public struct RoomMsgs
    {
        public int players_number;
        public int[] players_sequence, nobels_info, levelOneCards_info, levelTwoCards_info, levelThreeCards_info;
    };
}
