using System;

namespace MessageTools
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
        public static int HexConvertInt(byte[] bytes)
        {
            if (bytes == null) return 0;
            int result = 0;
            for (int i = 0; i < bytes.Length; i++)
                result = (result << 8) + bytes[i];
            return result;
        }
        public static long HexConvertLong(byte[] bytes)
        {
            if (bytes == null) return 0L;
            long result = 0L;
            for (int i = 0;i < bytes.Length; i++)
                result = (result << 8) + bytes[i];
            return result;
        }
    }
}
