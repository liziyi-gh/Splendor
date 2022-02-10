using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using MessageTools;

namespace Transmission
{
    public static class Client
    {
        public static void  Connect()
        {
            string host = "127.0.0.1";
            int port = 13204;
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipEnd = new IPEndPoint(ip, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipEnd);
            Thread th = new Thread(delegate() { Receive(socket); });
            th.Start();
        }
        public static void JsonStringTest()
        {
            string jsonText = @"{""input"" : ""value"", ""output"" : ""result""}";
            JsonReader reader = new JsonTextReader(new StringReader(jsonText));
            int i = 0;
            while (reader.Read())
            {
                i++;
                Console.WriteLine(i.ToString());
                Console.WriteLine(reader.TokenType + "\t\t" + reader.ValueType + "\t\t" + reader.Value);
            }
        }
        public static void TextTest()
        {
            long numText = 1024L;
            byte[] text = Tools.LongConvertHex(numText);
            byte[] cutSample = text.Skip(4).Take(4).ToArray();
            Console.WriteLine(Tools.HexConvertInt(cutSample));
        }
        public static void Receive(Socket socket)
        {
            while(true)
            {
                byte[] buffer = new byte[1024];
                int len = socket.Receive(buffer, buffer.Length, 0);
            }
            

        }
        public static void Main()
        {
            TextTest();
        }
    }
}

