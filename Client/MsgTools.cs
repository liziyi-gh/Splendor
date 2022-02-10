using System;
using MsgStruct;

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
            return result ;
        }
        public static RoomMsgs RoomMsgsProcess(byte[] buffer)
        {
            RoomMsgs msg = new RoomMsgs();
            //
            return msg;
        }
    }
}
