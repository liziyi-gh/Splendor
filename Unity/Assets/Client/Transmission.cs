using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MsgTools;
using MsgStruct;
using ApiID;
using GameRooms;
using Gems;
using CardLevelTypes;
using Logger;
using Players;

namespace Transmission
{
    public static class Client
    {
        public static Socket? socket;

        public static void Connect()
        {
            //string host = "127.0.0.1";
            string host = "175.178.115.8";
            int port = 13204;

            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipEnd = new IPEndPoint(ip, port);

            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipEnd);

            Logging.LogInit();
            Logging.LogConnect();

            GameRoom.LoadCardMsgJsonFile();

            Thread th = new Thread(delegate () { Receive(socket); });
            th.Start();
        }

        private static void Receive(Socket socket)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int head_len = socket.Receive(buffer, 0, 28, 0);

                if (head_len == 0) { Shutdown(); return; }

                Msgs head_msg = Tools.MsgHeadUnpack(buffer);

                string body_str = "";
                if (head_msg.msg_len > 28)
                {
                    int body_len = socket.Receive(buffer, 0, (int)head_msg.msg_len-28, 0);
                    body_str = Tools.MsgBodyUnpack(buffer, head_msg.msg_len);
                }

                Logging log = new Logging();
                log.LogMsg(head_msg, body_str, LogSwitch.RECEIVE);

                Msgs body_msg = new Msgs();
                switch (head_msg.api_id)
                {
                    case API_ID.INIT_RESP:
                        body_msg = Tools.MsgINIT_RESP(body_str);
                        GameManager.GetPlayerID(body_msg);
                        break;

                    case API_ID.PLAYER_READY:
                        GameManager.PlayerGetReady(head_msg);
                        break;

                    case API_ID.GAME_START:
                        GameRoom.GameRoomInit(Tools.MsgsGAME_START(body_str));
                        while (!GameRoom.reInit);
                        GameManager.GameStart();
                        break;

                    case API_ID.NEW_TURN:
                        body_msg = Tools.MsgNEW_TURN(body_str);
                        GameManager.NewTurn(body_msg);
                        break;

                    case API_ID.PLAYER_OPERATION:
                        GameRoom.UpdatePLAYER_OPERATION(out body_msg, head_msg, body_str);
                        GameManager.PlayerOperation(body_msg);
                        break;

                    case API_ID.PLAYER_OPERATION_INVALID:
                        GameManager.OperationInvalid();
                        break;

                    case API_ID.NEW_PLAYER:
                        GameManager.NewPlayerGetIn(head_msg);
                        break;

                    case API_ID.PLAYER_GET_NOBLE:
                        body_msg= Tools.MsgPLAYER_GET_NOBLE(body_str);
                        GameRoom.UpdatePLAYER_GET_NOBLE(body_msg);
                        GameManager.PlayerGetNoble(body_msg);
                        break;

                    case API_ID.NEW_CARD:
                        GameRoom.ShowNEW_CARD(Tools.MsgNEW_CARD(body_str));
                        GameManager.NewCard();
                        break;

                    case API_ID.DISCARD_GEMS:
                        body_msg = Tools.MsgDISCARD_GEMS(body_str);
                        body_msg.player_id = head_msg.player_id;
                        GameManager.DiscardGems();
                        break;

                    case API_ID.WINNER:
                        //
                        break;

                    default:
                        break;
                }
            }
        }

        public static void Send(Msgs msg)
        {
            List<byte> buffer = new List<byte>();
            msg.msg_len = 28;

            switch (msg.api_id)
            {
                case API_ID.INIT:
                    buffer.AddRange(Tools.MsgHeadPack(msg));
                    break;

                case API_ID.PLAYER_READY:
                    buffer.AddRange(Tools.MsgHeadPack(msg));
                    break;

                case API_ID.PLAYER_OPERATION:
                    buffer.AddRange(Tools.SendPlayerOperation(msg));
                    break;

                case API_ID.PLAYER_GET_NOBLE:
                    buffer.AddRange(Tools.SendPLAYER_GET_NOBLE(msg));
                    break;

                default:
                    break;
            }

            Logging log = new Logging();
            log.LogMsgSend(buffer.ToArray());

            socket.Send(buffer.ToArray());
        }

        public static void Shutdown()
        {
            socket.Close();
            Logging.LogClose();
        }
    }
}

