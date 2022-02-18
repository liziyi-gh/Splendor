using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

    public class JsonPLAYER_OPERATION
    {
        public ulong player_id { get; set; }
        public string operation_type { get; set; }
        public JArray operation_info { get; set; }

        public JsonPLAYER_OPERATION(ulong id, string type)
        {
            player_id = id;
            operation_type = type;
            operation_info = new JArray();
        }

        public void AddOperatonInfo<T>(T jo)
        {
            operation_info.Add(jo);
        }
    }

    public class JsonPLAYER_GET_NOBLE
    {
        public ulong player_id { get; set; }
        public List<int> noble_number { get; set; }

        public JsonPLAYER_GET_NOBLE(ulong id, List<int> nobles_id)
        {
            player_id = id;
            noble_number = nobles_id;
        }
    }

    public class JsonGems
    {
        public string gems_type { get; set; }
        public int gems_number { get; set; }

        public JsonGems(string type, int number)
        {
            gems_type = type;
            gems_number = number;
        }
    }
}