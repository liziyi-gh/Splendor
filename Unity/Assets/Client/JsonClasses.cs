using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JsonClasses
{
    ///<summary>
    ///JSON数据的实体类
    ///</summary>
    public class JsonRoom
    {
        public int players_number { get; set; }
        public ulong[]? players_sequence { get; set; }
        public int[]? nobles_info { get; set; }
        public int[]? levelOneCards_info { get; set; }
        public int[]? levelTwoCards_info { get; set; }
        public int[]? levelThreeCards_info { get; set; }
    }

    public class JsonINIT_RESP
    {
        public ulong allocated_player_id { get; set; }
        public ulong[]? other_player_id { get; set; }

    }

    public class JsonNEW_TURN
    {
        public ulong new_turn_player { get; set; }
    }
}