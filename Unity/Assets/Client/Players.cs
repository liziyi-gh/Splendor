using System;
using System.Collections.Generic;
using Gems;

namespace Players
{
    public class Player
    {
        public int id, point, foldCards_num;
        public int[]? cards, foldCards, nobels;
        public Dictionary<string, int> cards_type;
        public Dictionary<string, int> gems;
        public Player()
        {
            cards_type = new Dictionary<string, int>
            {
                { GEM.DIAMOND, 0 },
                { GEM.EMERALD, 0 },
                { GEM.OBSIDIAN, 0 },
                { GEM.RUBY, 0 },
                { GEM.SAPPHIRE, 0 }
            };
            gems = new Dictionary<string, int>
            {
                { GEM.DIAMOND, 0 },
                { GEM.EMERALD, 0 },
                { GEM.OBSIDIAN, 0 },
                { GEM.RUBY, 0 },
                { GEM.SAPPHIRE, 0 },
                { GEM.GOLDEN, 0 }
            };
            id = 0;
            point = 0;
            foldCards_num = 0;
        }
    }
}
