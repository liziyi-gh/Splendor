using System;
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
                    msg.card_id = (int)body_data["operation_type"][0]["card_number"];
                    for (i = 1; i < body_data["operation_info"].Count(); i++)
                        msg.gems[(string)body_data["operation_info"][i]["gems_type"]]
                            = (int)body_data["operation_info"][i]["gems_number"];
                    break;

                case Operation.FOLD_CARD:
                    msg.card_id = (int)body_data["operation_info"][0]["card_number"];
                    if (GameRoom.gems_last_num[GEM.GOLDEN] != 0) msg.gems[GEM.GOLDEN] = 1;
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    msg.card_level = (int)body_data["operation_info"][0]["card_level"];
                    msg.card_id = (int)body_data["operation_info"][0]["card_number"];
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
                msg.nobles_id.Add((int)i);
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
                    dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("card_number", msg.card_id)));

                    foreach (var i in typeof(GEM).GetFields())
                        if (msg.gems[i.Name.ToLower()] != 0)
                            dataPLAYER_OPERATION.AddOperatonInfo<JObject>(new JObject(new JProperty("gems_type", i.Name.ToLower()), new JProperty("gems_number", msg.gems[i.Name.ToLower()])));
                    break;

                case Operation.FOLD_CARD:
                    operationInfo.Add(new JObject(new JProperty("card_number", msg.card_id)));
                    break;

                case Operation.FOLD_CARD_UNKNOWN:
                    operationInfo.Add(new JObject(new JProperty("card_level", msg.card_level),
                                                  new JProperty("card_number", msg.card_id)));
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
            body_data.Add(new JArray(msg.nobles_id[0]));
            byte[] body_msg =  Encoding.UTF8.GetBytes(body_data.ToString());
            buffer.AddRange(MsgHeadPack(msg, (ulong)body_msg.Length));
            buffer.AddRange(body_msg);
            return buffer.ToArray();
        }
    }
}