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
        public int[]? players_sequence { get; set; }
        public int[]? nobels_info { get; set; }
        public int[]? levelOneCards_info { get; set; }
        public int[]? levelTwoCards_info { get; set; }
        public int[]? levelThreeCards_info { get; set; }
    }
    public class JsonINIT_RESP
    {
        public uint allocated_player_id { get;set; }
        public uint[]? other_player_id { get;set; }
       
    }
    public class JsonNEW_TURN
    {
        public uint new_turn_player { get;set; }
    }
    public class JsonPLAYER_OPERATION_Head
    {
        public uint player_id { get;set; }
        public string? operation_type { get;set; }
    }
    public class JsonGET_GEMS_Info
    {
        public List<JsonGemsInfo> operation_info { get; set; }
    }
    public class JsonBUY_CARD_Info
    {

    }
    public class JsonFOLD_CARD_Info
    {
        public List<JsonCardInfo> operation_info { get; set; }
    }
    public class JsonGemsInfo
    {
        public string? gems_type { get; set; }
        public int gems_number { get; set; }
    }
    public class JsonCardInfo
    {
        public int card_number { get; set; }
    }
}

