﻿using System;
using System.Linq;
using System.Text;
using System.Net;
using MsgStruct;
using PlayerOperations;
using Gems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonClasses;
using GameRooms;
using System.Collections.Generic;

namespace MsgTools
{
    public static class Tools
    {
        public static Msgs MsgHeadUnpack(byte[] buffer)
        {
            Msgs msg = new Msgs();
            msg.api_id = BitConverter.ToUInt32(buffer, 0);
            msg.player_id = BitConverter.ToUInt64(buffer, 4);
            msg.msg_len = BitConverter.ToUInt64(buffer, 12);
            return msg;
        }

        public static string MsgBodyUnpack(byte[] buffer, Msgs head_msg)
        {
            string body_str = Encoding.UTF8.GetString(buffer.Skip(28).Take((int)head_msg.msg_len-28).ToArray());
            return body_str;
        }

        public static byte[] MsgHeadPack(Msgs msg, ulong body_len)
        {
            byte[] result = new byte[28];
            for (int i = 0; i < 28; i++)
                result[i] = 0;

            byte[] lenArray;
            lenArray = BitConverter.GetBytes(body_len + 28);
            lenArray.CopyTo(result, 8);

            byte[] playeridArray;
            playeridArray = BitConverter.GetBytes(msg.player_id);
            playeridArray.CopyTo(result, 16);

            byte[] idArray;
            idArray = BitConverter.GetBytes(msg.api_id);
            idArray.CopyTo(result, 24);

            Array.Reverse(result);
            return result;
        }

        public static RoomMsgs MsgsGAME_START(string body_str)
        {
            JsonRoom roomData = JsonConvert.DeserializeObject<JsonRoom>(body_str);
            RoomMsgs roomMsgs = new RoomMsgs();
            roomMsgs.players_number = roomData.players_number;
            roomMsgs.players_sequence = roomData.players_sequence;
            roomMsgs.nobles_info = roomData.nobles_info;
            roomMsgs.levelOneCards_info = roomData.levelOneCards_info;
            roomMsgs.levelTwoCards_info = roomData.levelTwoCards_info;
            roomMsgs.levelThreeCards_info = roomData.levelThreeCards_info;
            return roomMsgs;
        }

        public static Msgs MsgINIT_RESP(string body_str)
        {
            Msgs msg = new Msgs();
            JsonINIT_RESP dataINIT_RESP = JsonConvert.DeserializeObject<JsonINIT_RESP>(body_str);
            msg.player_id = dataINIT_RESP.allocated_player_id;
            msg.other_player_id.AddRange(dataINIT_RESP.other_player_id);
            return msg;
        }

        public static Msgs MsgNEW_TURN(string body_str)
        {
            Msgs msg = new Msgs();
            JsonNEW_TURN dataNEW_TURN = JsonConvert.DeserializeObject<JsonNEW_TURN>(body_str);
            msg.player_id = dataNEW_TURN.new_turn_player;
            return msg;
        }

        public static Msgs MsgPLAYER_OPERATION(string body_str)
        {
            Msgs msg = new Msgs();
            JObject body_data = JObject.Parse(body_str);
            msg.player_id = (ulong)body_data["player_id"];
            msg.operation_type = (string)body_data["operation_type"];
            int i = 0;
            switch (msg.operation_type)
            {
                case Operation.GET_GEMS:
                    for (i = 0; i < body_data["operation_info"].Count(); i++)
                        msg.gems[(string)body_data["operation_info"][i]["gems_type"]]
                            = (int)body_data["operation_info"][i]["gems_number"];
                    break;

                case Operation.BUY_CARD:
                    msg.card_num = (int)body_data["operation_type"][0]["card_number"];
                    for (i = 1; i < body_data["operation_info"].Count(); i++)
                        msg.gems[(string)body_data["operation_info"][i]["gems_type"]]
                            = (int)body_data["operation_info"][i]["gems_number"];
                    break;

                case Operation.FOLD_CARD:
                    msg.card_num = (int)body_data["operation_info"][0]["card_number"];
                    if (GameRoom.gems_last_num[GEM.GOLDEN] != 0) msg.gems[GEM.GOLDEN] = 1;
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    msg.card_level = (int)body_data["operation_info"][0]["card_level"];
                    msg.card_num = (int)body_data["operation_info"][0]["card_number"];
                    break;

                default:
                    break;
            }
            return msg;
        }

        public static Msgs MsgPLAYER_GET_NOBLE(string body_str)
        {
            Msgs msg = new Msgs();
            JObject body_data = JObject.Parse(body_str);
            msg.player_id = (ulong)body_data["player_id"];
            foreach (var i in body_data["noble_number"])
                msg.noble_num.Add((int)i);
            return msg;
        }

        public static byte[] SendPlayerOperation(Msgs msg)
        {

            JObject body_data = new JObject();
            body_data.Add("player_id", msg.player_id);
            body_data.Add("operation_type", msg.operation_type);
            JArray operationInfo = new JArray();
            switch (msg.operation_type)
            {
                case Operation.GET_GEMS:
                    foreach (var i in typeof(GEM).GetProperties())
                        if (msg.gems[i.Name] != 0)
                            operationInfo.Add(new JObject(new JProperty("gems_type", i.Name),
                                                          new JProperty("gems_number", msg.gems[i.Name])));
                    break;

                case Operation.BUY_CARD:
                    operationInfo.Add(new JObject(new JProperty("card_number", msg.card_num)));
                    foreach (var i in typeof(GEM).GetProperties())
                        if (msg.gems[i.Name] != 0)
                            operationInfo.Add(new JObject(new JProperty("gems_type", i.Name),
                                                          new JProperty("gems_number", msg.gems[i.Name])));
                    break;

                case Operation.FOLD_CARD:
                    operationInfo.Add(new JObject(new JProperty("card_number", msg.card_num)));
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    operationInfo.Add(new JObject(new JProperty("card_level", msg.card_level),
                                                  new JProperty("card_number", msg.card_num)));
                    break;

                default:
                    break;
            }
            body_data.Add("operation_info", operationInfo);
            byte[] body_msg =  Encoding.UTF8.GetBytes(body_data.ToString());
            List<byte> buffer = new List<byte>();
            buffer.AddRange(MsgHeadPack(msg, (ulong)body_msg.Length));
            buffer.AddRange(body_msg);
            return buffer.ToArray();
        }

        public static byte[] SendPLAYER_GET_NOBLE(Msgs msg)
        {
            List<byte> buffer = new List<byte>();
            
            JObject body_data = new JObject();
            body_data.Add("player_id", msg.player_id);
            body_data.Add(new JArray(msg.noble_num[0]));
            byte[] body_msg =  Encoding.UTF8.GetBytes(body_data.ToString());
            buffer.AddRange(MsgHeadPack(msg, (ulong)body_msg.Length));
            buffer.AddRange(body_msg);
            return buffer.ToArray();
        }
    }
}