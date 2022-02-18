using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Generic;
using MsgStruct;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MsgTools;

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

        public static void LogInit()
        {
            path = ".\\Client.log";
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
                log.WriteLine(DateTime.Now.ToString("G") + "    "+logSwitch+"-->API:{0}, Player:{1}, MsgLength:{2}", msg.api_id, msg.player_id, msg.msg_len);
                log.WriteLine(body_msg);
            }
        }

        public void LogMsgSend(byte[] buffer)
        {
            Msgs head_msg = Tools.MsgHeadUnpack(buffer);
            string body_str = Tools.MsgBodyUnpack(buffer, head_msg.msg_len);
            lock(balanceLock)
            {
                log.WriteLine(DateTime.Now.ToString("G") + "    "+LogSwitch.SEND+"-->API:{0}, Player:{1}, MsgLength:{2}", head_msg.api_id, head_msg.player_id, head_msg.msg_len);
                log.WriteLine(body_str);
            }
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
