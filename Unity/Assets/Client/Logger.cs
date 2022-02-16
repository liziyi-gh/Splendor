using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Generic;
using MsgStruct;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Logger
{
    public static class Logging
    {
        
        public const string path = ".\\ClientLog.txt";

        public static void LogMsgHead(Msgs head_msg)
        {
            var log = new StreamWriter(path, true, System.Text.Encoding.Default);
            log.WriteLine("--------------------------------------------------");
            log.WriteLine(DateTime.Now.ToString("G") + "    API:{0}, Player:{1}, MsgLength:{2}", head_msg.api_id, head_msg.player_id, head_msg.msg_len);
            log.Flush();
            log.Close();
        }

        public static void LogMsgBody(byte[] buffer)
        {
            var log = new StreamWriter(path, true, System.Text.Encoding.Default);
            JObject msg = JObject.Parse(buffer.Skip(28).Take(buffer.Length).ToString());
            foreach (var i in msg.Properties())
                log.Write(i.ToString()+":"+msg[i].ToString()+", ");
            log.WriteLine();
            log.Flush();
            log.Close();
        }

        public static void LogBuffer(byte[] buffer, string tag)
        {
            var log = new StreamWriter(path, true, System.Text.Encoding.Default);
            log.WriteLine(tag + " buffer:");
            int j = 0;
            foreach (var i in buffer)
            {
                log.Write(i);
                if (j == 3)
                {
                    log.Write(", ");
                    j = 0;
                }
                else j++;
            }
            log.WriteLine() ;
            log.Flush();
            log.Close();
        }

        public static void LogAny<T>(T data)
        {
            var log = new StreamWriter(path, true, System.Text.Encoding.Default);
            log.WriteLine("player_id:"+data);
            log.Flush();
            log.Close();
        }
    }
}
