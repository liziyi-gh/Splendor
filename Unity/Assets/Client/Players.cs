using System;
using System.Collections.Generic;
using Gems;

namespace Players
{
    public class Player
    {
        public ulong id;
        public int point, foldCards_num;
        public List<int> cards, foldCards, nobles;
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
            cards = new List<int>();
            foldCards = new List<int>();
            nobles = new List<int>();
        }
    }

    public static class Operation
    {
        public const string GET_GEMS = "get_gems",
                            BUY_CARD = "buy_card",
                            FOLD_CARD = "fold_card",
                            FOLD_CARD_UNKNOWN = "fold_card_unknown",
                            DISCARD_GEMS = "discard_gems";
    }
}
