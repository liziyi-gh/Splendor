using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;
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
            string host = "127.0.0.1";
            int port = 13204;
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipEnd = new IPEndPoint(ip, port);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipEnd);

            Logging.LogInit();
            Logging.LogConnect();

            Thread th = new Thread(delegate () { Receive(socket); });
            th.Start();
        }

        public static void Receive(Socket socket)
        {
            while (true)
            {
                byte[] buffer = new byte[1024];
                int len = socket.Receive(buffer, buffer.Length, 0);
                Msgs head_msg = Tools.MsgHeadUnpack(buffer);
                Logging.LogMsg(buffer, "Receive");
                string body_str = "";
                if (head_msg.msg_len > 28) body_str = Tools.MsgBodyUnpack(buffer, head_msg);
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
                        RoomMsgs roomMsgs = Tools.MsgsGAME_START(body_str);
                        GameRoom.GameRoomInit(roomMsgs);
                        GameManager.GameStart(roomMsgs);
                        break;

                    case API_ID.NEW_TURN:
                        body_msg = Tools.MsgNEW_TURN(body_str);
                        GameManager.NewTurn(body_msg);
                        break;

                    case API_ID.PLAYER_OPERATION:
                        body_msg = Tools.MsgPLAYER_OPERATION(body_str);
                        CardPosition cardPos = GameRoom.GetCardPosition(body_msg.card_id);
                        int player_pos = Array.BinarySearch(GameRoom.players_sequence, body_msg.player_id);
                        switch (body_msg.operation_type)
                        {
                            case Operation.GET_GEMS:
                                foreach (var i in typeof(GEM).GetProperties())
                                {
                                    GameRoom.players[player_pos].gems[i.Name] += body_msg.gems[i.Name];
                                    GameRoom.gems_last_num[i.Name] -= body_msg.gems[i.Name];
                                }
                                break;

                            case Operation.BUY_CARD:
                                foreach (var i in typeof(GEM).GetProperties())
                                {
                                    GameRoom.players[player_pos].gems[i.Name] -= body_msg.gems[i.Name];
                                    GameRoom.gems_last_num[i.Name] += body_msg.gems[i.Name];
                                }
                                //fix me
                                GameRoom.cards_info[cardPos.cardLevel][cardPos.cardIndex] = 0;

                                GameRoom.players[player_pos].cards.Add(body_msg.card_id);   
                                //加分数
                                break;

                            case Operation.FOLD_CARD:
                                GameRoom.cards_info[cardPos.cardLevel][cardPos.cardIndex] = 0;
                                GameRoom.players[player_pos].gems[GEM.GOLDEN] += body_msg.gems[GEM.GOLDEN];
                                GameRoom.gems_last_num[GEM.GOLDEN] -= body_msg.gems[GEM.GOLDEN];
                                GameRoom.players[player_pos].foldCards_num++;
                                GameRoom.players[player_pos].foldCards.Add(body_msg.card_id);
                                break;

                            case Operation.FOLD_CARD_UNKNOWN:
                                break;

                            default:
                                break;
                        }    
                        //
                        break;

                    case API_ID.PLAYER_OPERATION_INVALID:
                        GameManager.OperationInvalid();
                        break;

                    case API_ID.NEW_PLAYER:
                        GameManager.NewPlayerGetIn(head_msg);
                        break;

                    case API_ID.PLAYER_GET_NOBLE:
                        body_msg= Tools.MsgPLAYER_GET_NOBLE(body_str);
                        switch (body_msg.nobles_id.Count())
                        {
                            case 1:
                                GameRoom.players[Array.BinarySearch(GameRoom.players_sequence, body_msg.player_id)].point += 3;
                                GameRoom.players[Array.BinarySearch(GameRoom.players_sequence, body_msg.player_id)].nobles.Add(body_msg.nobles_id[0]);
                                break;

                            default:
                                break;
                        }
                        //
                        break;

                    default:
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
                    buffer.AddRange(Tools.SendPlayerOperation(msg));
                    break;

                case API_ID.PLAYER_GET_NOBLE:
                    buffer.AddRange(Tools.SendPLAYER_GET_NOBLE(msg));
                    break;

                default:
                    break;
            }
            socket.Send(buffer.ToArray());
            Logging.LogMsg(buffer.ToArray(), "Send");
        }
    }
}

