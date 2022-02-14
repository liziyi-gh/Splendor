using System;
using Players;
using MsgStruct;
using Gems;
using JsonClasses;

namespace GameRooms
{
    public static class GameRoom
    {
        public static int players_number;
        public static ulong[] players_sequence;
        public static Dictionary<string, int[]> cards_info;
        public static Dictionary<string, int> gems_last_num, cards_last_num;
        public static List<Player> players;

        public static void GameRoomInit(RoomMsgs msg)
        {
            players_number = msg.players_number;
            players_sequence = msg.players_sequence;
            int gems_num = 7;
            if (players_number == 3) gems_num = 5;
            if (players_number == 2) gems_num = 4;
            gems_last_num = new Dictionary<string, int>
            {
                { GEM.DIAMOND, gems_num },
                { GEM.EMERALD, gems_num },
                { GEM.OBSIDIAN, gems_num },
                { GEM.SAPPHIRE, gems_num },
                { GEM.RUBY, gems_num },
                { GEM.GOLDEN, 5 }
            };
            cards_last_num = new Dictionary<string, int>
            {
                {"levelOneCards", 40 },
                {"levelTwoCards", 30 },
                {"levelThreeCards", 20 },
                {"nobles", players_number+1 }
            };
            cards_info = new Dictionary<string, int[]>
            {
                {"levelOneCards", msg.levelOneCards_info },
                {"levelTwoCards", msg.levelTwoCards_info },
                {"levelThreeCards", msg.levelThreeCards_info },
                {"nobles", msg.nobles_info }
            };
            players = new List<Player>();
            for (int i = 0; i < players_number; i++)
            {
                players.Add(new Player());
                players[i].id = players_sequence[i];
            }
        }
    }
}
