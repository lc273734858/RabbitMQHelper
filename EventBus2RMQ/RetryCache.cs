using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventBus2RMQ
{
    /// <summary>
    /// 消息
    /// </summary>
    public struct MessageInfo
    {
        /// <summary>
        /// 消息主体
        /// </summary>
        public string Message;
        /// <summary>
        /// 时间名称
        /// </summary>
        public string EventName;
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate;
        /// <summary>
        /// 是否持久
        /// </summary>
        public bool? Persistent;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="eventname"></param>
        /// <param name="persistent"></param>
        public MessageInfo(string message, string eventname, bool persistent)
        {
            this.Message = message;
            this.EventName = eventname;
            this.Persistent = persistent;
            CreateDate = DateTime.Now;
        }
        public override string ToString()
        {
            return string.Format("{2}|{0}|{1}", EventName, Message, CreateDate);
        }
    }
    /// <summary>
    /// 重试缓存
    /// </summary>
    static public class RetryCache
    {
        private static int retryCount = 3;
        private static int intervalTime = 2000;
        private static Queue<MessageInfo> Cache = new Queue<MessageInfo>();
        private static StreamWriter fileWriter;
        static RetryCache()
        {
            fileWriter = GetLogWriter();
            BeginAutoSendTask();
        }
        private static StreamWriter GetLogWriter()
        {
            var logpath = EventBus2RMQConfig.GetDefaultLogPath();
            if (Directory.Exists(logpath) == false)
            {
                Directory.CreateDirectory(logpath);
            }
            return new System.IO.StreamWriter(EventBus2RMQConfig.GetLogFilePath(), true);
        }
        /// <summary>
        /// 启动自动发送任务
        /// </summary>
        static void BeginAutoSendTask()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(intervalTime);
                    while (Cache.Count > 0)
                    {
                        int count = 0;
                        while (count < retryCount)
                        {
                            count++;
                            MessageInfo current;
                            try
                            {
                                current = Cache.Peek();
                                Service.PushData(current.Message, current.EventName, current.Persistent);
                                break;
                            }
                            catch (Exception)
                            {
                                //ignor
                            }
                        }
                        var message = Cache.Dequeue();
                        if (count > retryCount)
                        {
                            SaveMessage(message);
                        }
                    }
                }
            });
        }
        static void SaveMessage(MessageInfo message)
        {
            fileWriter.WriteLine(message.ToString());
            fileWriter.Flush();
        }
        /// <summary>
        /// 添加缓存
        /// </summary>
        /// <param name="data"></param>
        /// <param name="eventname"></param>
        /// <param name="persistent"></param>
        static public void AddCache(string data, string eventname, bool persistent)
        {
            Cache.Enqueue(new MessageInfo(data, eventname, persistent));
        }
    }
}
