using System;
//using Newtonsoft.Json.Linq;
using UniversalTestAux;

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
            int[] test = { 1, 2, 3 };

            Console.WriteLine(Array.BinarySearch(test,4));
        }
    }
}
