using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using MsgTools;
using MsgStruct;
using ApiID;
using GameRooms;

namespace Transmission
{
    public class Client
    {
        public static Socket? socket;
        public static void  Connect()
        {
            string host = "127.0.0.1";
            int port = 13204;
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipEnd = new IPEndPoint(ip, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipEnd);
            Thread th = new Thread(delegate() { Receive(socket); });
            th.Start();
        }
        public static void Receive(Socket socket)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int len = socket.Receive(buffer, buffer.Length, 0);
                uint api_id = BitConverter.ToUInt32(buffer, 0);
                switch (api_id)
                {
                    case API_ID.GAME_START:
                        RoomMsgs roomMsgs = Tools.RoomMsgsProcess(buffer);
                        GameRoom gameRoom = new GameRoom(roomMsgs);
                        break;
                        //
                }
            }

        }
        public static void Send(Msgs msg)
        {
            List<byte> buffer = new List<byte>();
            switch (msg.api_id)
            {
                case API_ID.INIT:
                    buffer.AddRange(Tools.MsgHeadPack(msg, 0L));
                    break;
                case API_ID.PLAYER_READY:
                    buffer.AddRange(Tools.MsgHeadPack(msg, 0L));
                    break;
                case API_ID.PLAYER_OPERATION:
                    buffer.AddRange(Tools.PlayerOperation(msg));
                    break ;
            }
            socket.Send(buffer.ToArray());
            
        }
        public static void JsonStringTest()
        {
            string jsonText = @"{""input"" : ""value"", ""output"" : ""result""}";
            string str = "{'input':'value','output':'result'}";
            JsonReader reader = new JsonTextReader(new StringReader(str));
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
            
        }
        public static void Main()
        {
            JsonStringTest();
        }
    }
}

