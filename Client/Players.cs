using System;

namespace Players
{
    public class Player
    {
        public int id, point, foldCards_num;
        public int[]? cards, foldCards, nobels;
        public Dictionary<string, int> cardsType;
        public Dictionary<string, int> chips;
        public Player()
        {
            cardsType = new Dictionary<string, int>
            {
                { "ruby", 0 },
                { "diamond", 0 },
                { "sapphire", 0 },
                { "emerald", 0 },
                { "obsidian", 0 }
            };
            chips = new Dictionary<string, int>
            {
                { "ruby", 0 },
                { "diamond", 0 },
                { "sapphire", 0 },
                { "emerald", 0 },
                { "obsidian", 0 },
                { "golden", 0 }
            };
            id = 0;
            point = 0;
            foldCards_num = 0;
        }
    }
}
