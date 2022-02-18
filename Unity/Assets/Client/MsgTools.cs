﻿using System;
using System.Linq;
using System.Text;
using System.Net;
using MsgStruct;
using Gems;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonClasses;
using GameRooms;
using System.Collections.Generic;
using Players;

namespace MsgTools
{

    public static class Tools
    {

        public static Msgs MsgHeadUnpack(byte[] buffer)
        {
            Msgs msg = new Msgs();

            byte[] arr4 = buffer.Take(4).ToArray();
            Array.Reverse(arr4);
            msg.api_id = BitConverter.ToUInt32(arr4, 0);

            byte[] arr8 = buffer.Skip(4).Take(8).ToArray();
            Array.Reverse(arr8);
            msg.player_id = BitConverter.ToUInt64(arr8, 0);

            arr8 = buffer.Skip(12).Take(8).ToArray();
            Array.Reverse (arr8);
            msg.msg_len = BitConverter.ToUInt64(arr8, 0);

            return msg;
        }

        public static string MsgBodyUnpack(byte[] buffer, ulong msg_len)
        {
            string body_str = Encoding.UTF8.GetString(buffer.Skip(28).Take((int)msg_len-28).ToArray());

            return body_str;
        }

        public static byte[] MsgHeadPack(Msgs msg, ulong body_len)
        {
            byte[] result = new byte[28];

            byte[] lenArray = BitConverter.GetBytes(body_len + 28);;
            lenArray.CopyTo(result, 8);

            byte[] playeridArray = BitConverter.GetBytes(msg.player_id);
            playeridArray.CopyTo(result, 16);

            byte[] idArray = BitConverter.GetBytes(msg.api_id);
            idArray.CopyTo(result, 24);

            Array.Reverse(result);
            return result;
        }

        public static JsonRoom MsgsGAME_START(string body_str)
        {
            return JsonConvert.DeserializeObject<JsonRoom>(body_str);
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
            JsonPLAYER_OPERATION dataPLAYER_OPERATION = JsonConvert.DeserializeObject<JsonPLAYER_OPERATION>(body_str);

            msg.player_id = dataPLAYER_OPERATION.player_id;
            msg.operation_type  = dataPLAYER_OPERATION.operation_type;
            
            switch (msg.operation_type)
            {
                case Operation.GET_GEMS:
                    foreach (var i in dataPLAYER_OPERATION.operation_info)
                        msg.gems[(string)i["gems_type"]] = (int)i["gems_number"];
                    break;

                case Operation.BUY_CARD:
                    msg.card_id = (int)dataPLAYER_OPERATION.operation_info[0]["card_number"];
                    foreach (var i in dataPLAYER_OPERATION.operation_info)
                        msg.gems[(string)i["gems_type"]] = (int)i["gems_number"];
                    break;

                case Operation.FOLD_CARD:
                    msg.card_id = (int)dataPLAYER_OPERATION.operation_info[0]["card_number"];
                    if (GameRoom.gems_last_num[GEM.GOLDEN] != 0) msg.gems[GEM.GOLDEN] = 1;
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    msg.card_level = (int)dataPLAYER_OPERATION.operation_info[0]["card_level"];
                    msg.card_id = (int)dataPLAYER_OPERATION.operation_info[0]["card_number"];
                    break;

                default:
                    break;
            }
            return msg;
        }

        public static Msgs MsgPLAYER_GET_NOBLE(string body_str)
        {
            Msgs msg = new Msgs();
            JsonPLAYER_GET_NOBLE dataPLAYER_GET_NOBLE = JsonConvert.DeserializeObject<JsonPLAYER_GET_NOBLE>(body_str);

            msg.player_id = dataPLAYER_GET_NOBLE.player_id;
            msg.nobles_id = dataPLAYER_GET_NOBLE.noble_number;

            return msg;
        }

        public static Msgs MsgNEW_CARD(string body_str)
        {
            JObject dataNEW_CARD = JObject.Parse(body_str);
            
            Msgs msg = new Msgs();
            msg.card_id = dataNEW_CARD["card_number"];
            
            return msg;
        }

        public static byte[] SendPlayerOperation(Msgs msg)
        {
            List<byte> buffer = new List<byte>();
            JsonPLAYER_OPERATION dataPLAYER_OPERATION  = new JsonPLAYER_OPERATION(msg.player_id, msg.operation_type);

            switch (msg.operation_type)
            {
                case Operation.GET_GEMS:
                    foreach (var i in typeof(GEM).GetProperties())
                        if (msg.gems[i.Name] != 0)
                            dataPLAYER_OPERATION.AddOperatonInfo<JsonGems>(new JsonGems(i.Name, msg.gems[i.Name]));
                    break;

                case Operation.BUY_CARD:
                    dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("card_number", msg.card_id)));

                    foreach (var i in typeof(GEM).GetProperties())
                        if (msg.gems[i.Name] != 0)
                            dataPLAYER_OPERATION.AddOperatonInfo<JsonGems>(new JsonGems(i.Name, msg.gems[i.Name]));
                    break;

                case Operation.FOLD_CARD:
                    dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("card_number", msg.card_id)));
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("card_level", msg.card_level),
                                                                              new JProperty("card_number", msg.card_id)));
                    break;

                default:
                    break;
            }

            byte[] body_msg =  Encoding.UTF8.GetBytes(dataPLAYER_OPERATION.ToString());

            buffer.AddRange(MsgHeadPack(msg, (ulong)body_msg.Length));
            buffer.AddRange(body_msg);

            return buffer.ToArray();
        }

        public static byte[] SendPLAYER_GET_NOBLE(Msgs msg)
        {
            List<byte> buffer = new List<byte>();
            JsonPLAYER_GET_NOBLE dataPLAYER_GET_NOBLE = new JsonPLAYER_GET_NOBLE(msg.player_id, msg.nobles_id);

            byte[] body_msg =  Encoding.UTF8.GetBytes(dataPLAYER_GET_NOBLE.ToString());

            buffer.AddRange(MsgHeadPack(msg, (ulong)body_msg.Length));
            buffer.AddRange(body_msg);
            
            return buffer.ToArray();
        }
    }
}