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
}

