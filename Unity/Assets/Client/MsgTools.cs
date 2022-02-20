using System;
using System.IO;
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
using Logger;
using UnityEngine;

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
            string body_str = Encoding.UTF8.GetString(buffer.Take((int)msg_len-28).ToArray());

            return body_str;
        }

        public static byte[] MsgHeadPack(Msgs msg)
        {
            byte[] result = new byte[28];

            byte[] lenArray = BitConverter.GetBytes(msg.msg_len);
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
                    for (int i = 1; i<dataPLAYER_OPERATION.operation_info.Count(); i++)
                        msg.gems[(string)dataPLAYER_OPERATION.operation_info[i]["gems_type"]] = (int)dataPLAYER_OPERATION.operation_info[i]["gems_number"];
                    break;

                case Operation.FOLD_CARD:
                    msg.card_id = (int)dataPLAYER_OPERATION.operation_info[0]["card_number"];
                    msg.gems[GEM.GOLDEN] = (int)dataPLAYER_OPERATION.operation_info[1]["golden_number"];
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    msg.card_level = (int)dataPLAYER_OPERATION.operation_info[0]["card_level"];
                    msg.card_id = (int)dataPLAYER_OPERATION.operation_info[0]["card_number"];
                    if (GameRoom.gems_last_num[GEM.GOLDEN] != 0) msg.gems[GEM.GOLDEN] = 1;
                    break;

                default:
                    break;
            }
            return msg;
        }

        public static Msgs MsgPLAYER_OPERATION_DISCARD_GEMS(string body_str)
        {
            Msgs msg = new Msgs();
            JsonPLAYER_OPERATION_DISCARD_GEMS dataPLAYER_OPERATION_DISCARD_GEMS = JsonConvert.DeserializeObject<JsonPLAYER_OPERATION_DISCARD_GEMS>(body_str);

            msg.player_id = dataPLAYER_OPERATION_DISCARD_GEMS.player_id;
            msg.operation_type = dataPLAYER_OPERATION_DISCARD_GEMS.operation_type;

            foreach (var i in dataPLAYER_OPERATION_DISCARD_GEMS.operation_info.Properties())
                msg.gems[i.Name] = (int)dataPLAYER_OPERATION_DISCARD_GEMS.operation_info[i.Name];
            
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
            msg.card_id = (int)dataNEW_CARD["card_number"];
            
            return msg;
        }

        public static Msgs MsgDISCARD_GEMS(string body_str)
        {
            JObject dataDISCARD_GEMS = JObject.Parse(body_str);

            Msgs msg = new Msgs();
            msg.number_to_discard = (int)dataDISCARD_GEMS["number_to_discard"];

            return msg;
        }

        public static byte[] SendPlayerOperation(Msgs msg)
        {
            List<byte> buffer = new List<byte>();
            JsonPLAYER_OPERATION dataPLAYER_OPERATION  = new JsonPLAYER_OPERATION(msg.player_id, msg.operation_type);

            switch (msg.operation_type)
            {
                case Operation.GET_GEMS:
                    foreach (var i in typeof(GEM).GetFields())
                        if (msg.gems[i.Name.ToLower()] != 0)
                            dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("gems_type", i.Name.ToLower()), new JProperty("gems_number", msg.gems[i.Name.ToLower()])));
                    break;

                case Operation.BUY_CARD:
                    dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("card_number", msg.card_id)));

                    foreach (var i in typeof(GEM).GetFields())
                        if (msg.gems[i.Name.ToLower()] != 0)
                            dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("gems_type", i.Name.ToLower()), new JProperty("gems_number", msg.gems[i.Name.ToLower()])));
                    break;

                case Operation.FOLD_CARD:
                    dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("card_number", msg.card_id)));
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("card_level", msg.card_level),
                                                                              new JProperty("card_number", msg.card_id)));
                    break;

                case Operation.DISCARD_GEMS:
                    return SendPLAYER_OPERATION_DISCARD_GEMS(msg);
                    break;

                default:
                    break;
            }

            byte[] body_msg = JsonToBytes<JsonPLAYER_OPERATION>(dataPLAYER_OPERATION);

            msg.msg_len += (ulong)body_msg.Length;

            buffer.AddRange(MsgHeadPack(msg));
            buffer.AddRange(body_msg);

            return buffer.ToArray();
        }

        private static byte[] SendPLAYER_OPERATION_DISCARD_GEMS(Msgs msg)
        {
            JsonPLAYER_OPERATION_DISCARD_GEMS dataPLAYER_OPERATION_DISCARD_GEMS = new JsonPLAYER_OPERATION_DISCARD_GEMS(msg.player_id);

            foreach (var i in typeof(GEM).GetFields())
                if (msg.gems[i.Name.ToLower()] != 0)
                    dataPLAYER_OPERATION_DISCARD_GEMS.operation_info.Add(new JProperty(i.Name.ToLower(), msg.gems[i.Name.ToLower()]));

            byte[] body_msg = JsonToBytes<JsonPLAYER_OPERATION_DISCARD_GEMS>(dataPLAYER_OPERATION_DISCARD_GEMS);

            msg.msg_len += (ulong)body_msg.Length;

            List<byte> buffer = new List<byte>();
            buffer.AddRange(MsgHeadPack(msg));
            buffer.AddRange(body_msg);

            return buffer.ToArray();
        }

        public static byte[] SendPLAYER_GET_NOBLE(Msgs msg)
        {
            List<byte> buffer = new List<byte>();
            JsonPLAYER_GET_NOBLE dataPLAYER_GET_NOBLE = new JsonPLAYER_GET_NOBLE(msg.player_id, msg.nobles_id);

            byte[] body_msg = JsonToBytes<JsonPLAYER_GET_NOBLE>(dataPLAYER_GET_NOBLE);
            msg.msg_len += (ulong)body_msg.Length;

            buffer.AddRange(MsgHeadPack(msg));
            buffer.AddRange(body_msg);
            
            return buffer.ToArray();
        }

        private static byte[] JsonToBytes<T>(T jsonObject)
        {
            StringWriter sw = new StringWriter();
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(new JsonTextWriter(sw), jsonObject);

            return Encoding.UTF8.GetBytes(sw.GetStringBuilder().ToString());
        }

        public static int ReadCardPoint(int card_id)
        {
            return (int)GameRoom.jsonAllCardMsg[card_id-1]["points"];
        }

        public static string ReadCardType(int card_id)
        {
            return (string)GameRoom.jsonAllCardMsg[card_id-1]["gem_type"];
        }
    }
}