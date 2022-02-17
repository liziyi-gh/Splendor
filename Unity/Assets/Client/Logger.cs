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
        
        public static string path;
        public static StreamWriter log;
        
        public static void LogInit()
        {
            path = ".\\ClientLog.txt";
            log = new StreamWriter(path, true, System.Text.Encoding.Default);
            //log.AutoFlush = true;
        }

        public static void LogMsg(byte[] buffer, string logSwitch)
        {
            Msgs msg = Tools.MsgHeadUnpack(buffer);
            log.WriteLine(DateTime.Now.ToString("G") + "    "+logSwitch+"-->API:{0}, Player:{1}, MsgLength:{2}", msg.api_id, msg.player_id, msg.msg_len);
            log.WriteLine(Encoding.UTF8.GetString(buffer.Skip(28).Take(buffer.Length).ToArray()));
            log.Flush();
            
        }

        public static void LogConnect()
        {
            log.WriteLine("--------------------------------------------------------------------------");
            log.WriteLine(DateTime.Now.ToString("G") + "    Client connected");
            log.WriteLine("--------------------------------------------------------------------------");
            log.Flush();
        }

        public static void LogAny<T>(T data)
        {
            log.WriteLine("player_id:"+data);
            log.Flush();
            log.Close();
        }
    }
}
