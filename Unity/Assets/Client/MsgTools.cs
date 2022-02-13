using System;
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
            string body_str = Encoding.UTF8.GetString(buffer.Skip(28).Take((int)head_msg.msg_len).ToArray());
            return body_str;
        }
        public static byte[] MsgHeadPack(Msgs msg, ulong body_len)
        {
            byte[] result = new byte[28];
            for (int i = 0; i < 28; i++)
                result[i] = 0;

            byte[] idArray;
            idArray = BitConverter.GetBytes(msg.api_id);
            Array.Reverse(idArray);
            idArray.CopyTo(result, 0);

            byte[] playeridArray;
            playeridArray = BitConverter.GetBytes(msg.player_id);
            Array.Reverse(playeridArray);
            playeridArray.CopyTo(result, 4);

            byte[] lenArray;
            lenArray = BitConverter.GetBytes(body_len+28);
            Array.Reverse(lenArray);
            lenArray.CopyTo(result, 12);

            return result;
        }
        public static RoomMsgs MsgsGAME_START(string body_str)
        {
            RoomMsgs msg = new RoomMsgs();
            JsonRoom roomData = JsonConvert.DeserializeObject<JsonRoom>(body_str);
            msg.players_number = roomData.players_number;
            msg.players_sequence = roomData.players_sequence;
            msg.nobels_info = roomData.nobels_info;
            msg.levelOneCards_info = roomData.levelOneCards_info;
            msg.levelTwoCards_info = roomData.levelTwoCards_info;
            msg.levelThreeCards_info = roomData.levelThreeCards_info;
            return msg;
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
        public static Msgs MsgPLAYER_OPERATION(string body_str, GameRoom gameRoom)
        {
            Msgs msg = new Msgs();
            JObject body_data = JObject.Parse(body_str);
            msg.player_id = (ulong)body_data["player_id"];
            msg.operation_type = (string)body_data["operation_type"];
            int i = 0;
            switch (msg.operation_type)
            {
                case Operation.GET_GEMS:
                    for (i = 0;i< body_data["operation_info"].Count();i++)
                        msg.gems[(string)body_data["operation_info"][i]["gems_type"]] = (int)body_data["operation_info"][i]["gems_number"];
                    break;
                case Operation.BUY_CARD:
                    msg.card_num = (int)body_data["operation_type"][0]["card_number"];
                    for (i = 1; i < body_data["operation_info"].Count(); i++)
                        msg.gems[(string)body_data["operation_info"][i]["gems_type"]] = (int)body_data["operation_info"][i]["gems_number"];
                    break;
                case Operation.FOLD_CARD:
                    msg.card_num = (int)body_data["operation_info"][0]["card_number"];
                    if (gameRoom.gems_last_num[GEM.GOLDEN] != 0) msg.gems[GEM.GOLDEN] = 1;
                    break;
            }
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
                        if (msg.gems[i.Name]!=0)
                            operationInfo.Add(new JObject(new JProperty("gems_type", i.Name), new JProperty("gems_number", msg.gems[i.Name])));
                    break;
                case Operation.BUY_CARD:
                    operationInfo.Add(new JObject(new JProperty("card_number", msg.card_num)));
                    foreach (var i in typeof(GEM).GetProperties())
                        if (msg.gems[i.Name] != 0)
                            operationInfo.Add(new JObject(new JProperty("gems_type", i.Name), new JProperty("gems_number", msg.gems[i.Name])));
                    break;
                case Operation.FOLD_CARD:
                    operationInfo.Add(new JObject(new JProperty("card_number", msg.card_num)));
                    break;
            }
            body_data.Add("operation_type", operationInfo);
            return Encoding.UTF8.GetBytes(body_data.ToString());
        }    
    }
}