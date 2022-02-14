﻿using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using MsgTools;
using MsgStruct;
using ApiID;
using GameRooms;
using PlayerOperations;
using Gems;

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

                string body_str = "";
                if (head_msg.msg_len > 28) body_str = Tools.MsgBodyUnpack(buffer, head_msg);

                Msgs body_msg = new Msgs();
                switch (head_msg.api_id)
                {
                    case API_ID.INIT_RESP:
                        body_msg = Tools.MsgINIT_RESP(body_str);
                        //
                        break;

                    case API_ID.PLAYER_READY:
                        //
                        break;

                    case API_ID.GAME_START:
                        GameRoom.GameRoomInit(Tools.MsgsGAME_START(body_str));
                        break;

                    case API_ID.NEW_TURN:
                        body_msg = Tools.MsgNEW_TURN(body_str);
                        //
                        break;

                    case API_ID.PLAYER_OPERATION:
                        body_msg = Tools.MsgPLAYER_OPERATION(body_str);
                        switch (body_msg.operation_type)
                        {
                            case Operation.GET_GEMS:
                                foreach (var i in typeof(GEM).GetProperties())
                                {
                                    GameRoom.players[Array.BinarySearch(GameRoom.players_sequence, body_msg.player_id)].gems[i.Name] += body_msg.gems[i.Name];
                                    GameRoom.gems_last_num[i.Name] -= body_msg.gems[i.Name];
                                }
                                break;

                            case Operation.BUY_CARD:
                                foreach (var i in typeof(GEM).GetProperties())
                                    GameRoom.players[Array.BinarySearch(GameRoom.players_sequence, body_msg.player_id)].gems[i.Name] -= body_msg.gems[i.Name];
                                break;

                            case Operation.FOLD_CARD:
                                break;

                            case Operation.FOLD_CARD_UNKNOWN:
                                break;

                            default:
                                break;
                        }    
                        //
                        break;

                    case API_ID.PLAYER_OPERATION_INVALID:
                        //
                        break;

                    case API_ID.NEW_PLAYER:
                        
                        //
                        break;

                    case API_ID.PLAYER_GET_NOBLE:
                        body_msg= Tools.MsgPLAYER_GET_NOBLE(body_str);
                        switch (body_msg.noble_num.Count())
                        {
                            case 1:
                                GameRoom.players[Array.BinarySearch(GameRoom.players_sequence, body_msg.player_id)].point += 3;
                                GameRoom.players[Array.BinarySearch(GameRoom.players_sequence, body_msg.player_id)].nobles.Add(body_msg.noble_num[0]);
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
        }
    }
}

