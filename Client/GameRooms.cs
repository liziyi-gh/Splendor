using System;
using Players;
using MsgStruct;

namespace GameRooms
{
    public class GameRoom
    {
        public int players_number;
        public int[]? players_sequence, nobels_info, levelOneCards_info, levelTwoCards_info, levelThreeCards_info;
        public int levelOneCards_last_num, levelTwoCards_last_num, levelThreeCards_last_num, nobels_last_num;
        public Dictionary<string, int>? chips_last_num;
        Player[]? players;
        public GameRoom(RoomMsgs msg)
        {
            
            //
        }
    }
}
