using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UniversalTestAux;
using Logger;
using MsgStruct;

namespace UniversalTest
{
    public static class staticClass
    {
        public static int i = 0;
    }
    public class Test
    {
        public static void Main()
        {
            string str = "{\"A Test\":1}";
            byte[] data = Encoding.UTF8.GetBytes(str);
            Console.WriteLine(Encoding.UTF8.GetString(data.Skip(0).Take(30).ToArray()));
        }
    }
}
