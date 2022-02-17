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

    public static class Logging
    {
        
        private static string path;
        private static StreamWriter log;
        
        public static void LogInit()
        {
            path = ".\\Client.log";
            log = new StreamWriter(path, true, System.Text.Encoding.Default);
            log.AutoFlush = true;
        }

        public static void LogMsg(Msgs msg, string body_msg, string logSwitch)
        {
            log.WriteLine(DateTime.Now.ToString("G") + "    "+logSwitch+"-->API:{0}, Player:{1}, MsgLength:{2}", msg.api_id, msg.player_id, msg.msg_len);
            log.WriteLine(body_msg);
        }

        public static void LogConnect()
        {
            log.WriteLine("--------------------------------------------------------------------------");
            log.WriteLine(DateTime.Now.ToString("G") + "    Client connected");
            log.WriteLine("--------------------------------------------------------------------------");
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
