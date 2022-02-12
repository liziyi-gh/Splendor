using System;
using Newtonsoft.Json.Linq;

namespace UniversalTest
{
    public class Test
    {
        public static void Main()
        {
            string jsonText = "{\"operation_type\":\"buy_card\",\"operation_info\":[{\"card_number\":\"1\"},{\"gems_type\":\"sapphire\",\"gems_number\":\"1\"}]}";
            JObject o = JObject.Parse(jsonText);
        }
    }
}
