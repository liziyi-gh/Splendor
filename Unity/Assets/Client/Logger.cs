using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MsgStruct;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MsgTools;
using ApiID;

namespace Logger
{

    public static class LogSwitch
    {
        public const string RECEIVE = "Receive";
        public const string SEND = "Send";
    }

    public class Logging
    {
        
        private static string path;
        private static StreamWriter log;
        private readonly object balanceLock = new Object();

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr h);

        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        private static bool IsOccupied(string path)
        {
            IntPtr ptr = _lopen(path, OF_READWRITE | OF_SHARE_DENY_NONE);
            CloseHandle(ptr);
            return ptr == HFILE_ERROR;
        }

        private static string PathName(int num)
        {
            if (num == 1) return @".\Client.log";
            else return ".\\Client_"+num.ToString()+".log";
        }

        public static void LogInit()
        {
            
            int num = 1;
            while(File.Exists(PathName(num))&&IsOccupied(PathName(num))) num++;
            path = PathName(num);
            
            log = new StreamWriter(path, true, System.Text.Encoding.Default);
            log.AutoFlush = true;
        }

        public static void LogConnect()
        {
            log.WriteLine("--------------------------------------------------------------------------");
            log.WriteLine(DateTime.Now.ToString("G") + "    Client connected");
            log.WriteLine("--------------------------------------------------------------------------");
        }

        public void LogMsg(Msgs msg, string body_msg, string logSwitch)
        {
            lock(balanceLock)
            {
                if (msg.api_id == API_ID.NEW_TURN) log.WriteLine();
                log.WriteLine(DateTime.Now.ToString("G") + "    "+logSwitch+"-->API:{0}, Player:{1}, MsgLength:{2}", msg.api_id, msg.player_id, msg.msg_len);
                log.WriteLine(body_msg);
            }
        }

        public void LogMsgSend(byte[] buffer)
        {
            Msgs head_msg = Tools.MsgHeadUnpack(buffer);
            string body_str = (head_msg.msg_len > 28) ? Tools.MsgBodyUnpack(buffer.Skip(28).Take((int)head_msg.msg_len-28).ToArray(), head_msg.msg_len) : "";
            LogMsg(head_msg, body_str, LogSwitch.SEND);
        }

        public static void LogAny(string str)
        {
            log.WriteLine(str);
        }

        public static void LogClose()
        {
            log.Close();
        }
    }
}
