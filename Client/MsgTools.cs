using System;
using System.Linq;
using System.Text;
using System.Net;
using MsgStruct;
using Operations;
using Gems;
using Newtonsoft.Json;
using JsonClasses;

namespace MsgTools
{
    public static class Tools
    {
        public static byte[] IntConvertHex(int velocity)
        {
            int vel = velocity;
            byte[] result = new byte[4];
            for (int i = 3; i >= 0; i--)
                result[3 - i] = (byte)((vel >> (i * 8)) & 0xff);
            return result;
        }
        public static byte[] LongConvertHex(long velocity)
        {
            long vel = velocity;
            byte[] result = new byte[8];
            for (int i = 7; i >= 0; i--)
                result[7 - i] = (byte)((vel >> (i * 8)) & 0xff);
            return result;
        }
        public static RoomMsgs RoomMsgsProcess(byte[] buffer)
        {
            RoomMsgs msg = new RoomMsgs();
            ulong body_len  = BitConverter.ToUInt64(buffer, 12)-28L;
            string body_data = Encoding.UTF8.GetString(buffer.Skip(28).Take((int)body_len).ToArray());
            JsonRoom roomData = JsonConvert.DeserializeObject<JsonRoom>(body_data);
            msg.players_number = roomData.players_number;
            msg.players_sequence = roomData.players_sequence;
            msg.nobels_info = roomData.nobels_info;
            msg.levelOneCards_info = roomData.levelOneCards_info;
            msg.levelTwoCards_info = roomData.levelTwoCards_info;
            msg.levelThreeCards_info = roomData.levelThreeCards_info;
            return msg;
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
        public static byte[] PlayerOperation(Msgs msg)
        {
            string body_string;
            body_string = "{\"player_id\":" + msg.player_id.ToString() + ",\"operation_type\":\"" + msg.operation_type + "\",\"operation_info\":[";
            switch (msg.operation_type)
            {
                case Operation.GET_GEMS:
                    body_string = body_string + GetGems(msg);
                    break;
                case Operation.BUY_CARD:
                    body_string = body_string + BuyCard(msg);
                    break ;
                case Operation.FOLD_CARD:
                    body_string = body_string + FoldCard(msg);
                    break;
            }
            body_string = body_string + "]}";
            byte[] body_data = Encoding.UTF8.GetBytes(body_string);
            byte[] head_data = MsgHeadPack(msg, (ulong)body_data.Length);
            List<byte> result = new List<byte>();
            result.AddRange(head_data);
            result.AddRange(body_data);
            return result.ToArray();
        }    
        private static string GetGems(Msgs msg)
        {
            bool sign = false;
            string str = "";
            foreach (var i in typeof(GEM).GetProperties())
                if (msg.gems[i.Name] != 0)
                {
                    if (sign) str = str + ",";
                    else sign = true;
                    str = str + "{\"gems_type\":\"" + i.Name + "\",\"gems_number\":" + msg.gems[i.Name].ToString() + "}";
                }
            return str;
        }
        private static string BuyCard(Msgs msg)
        {
            string str = "{\"card_number\":" + msg.card_num.ToString() + "}";
            foreach (var i in typeof(GEM).GetProperties())
                if (msg.gems[i.Name] != 0)
                    str = str + ",{\"gems_type\":\"" + i.Name + "\",\"gems_number\":" + msg.gems[i.Name].ToString() + "}";
            return str;
        }
        private static string FoldCard(Msgs msg)
        {
            string str = "{\"card_number\":" + msg.card_num.ToString() + "}";
            return str;
        }
    }
}