using System;
using Players;
using MsgStruct;
using Gems;
using System.Collections.Generic;

namespace GameRooms
{
    public class GameRoom
    {
        public int players_number;
        public int[]? players_sequence;
        public Dictionary<string, int[]?> cards_info;
        public Dictionary<string, int> gems_last_num, cards_last_num;
        Player[]? players;
        public GameRoom(RoomMsgs msg)
        {
            players_number = msg.players_number;
            players_sequence = msg.players_sequence;
            if (players_number != 0)
            {
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
                    {"nobels", players_number+1 }
                };
                cards_info = new Dictionary<string, int[]?>
                {
                    {"levelOneCards", msg.levelOneCards_info },
                    {"levelTwoCards", msg.levelTwoCards_info },
                    {"levelThreeCards", msg.levelThreeCards_info },
                    {"nobels", msg.nobels_info }
                };
                players = new Player[players_number];
                for (int i = 0; i < players_number; i++)
                {
                    players[i] = new Player();
                    players[i].id = players_sequence[i];
                }
            }
            
        }
    }
}
