using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EventBus2RMQ
{
    public class EventBus2RMQConfig
    {
        /// <summary>
        /// MQ地址
        /// </summary>
        public string RabbitMQAdress { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string PassWord { get; set; }
        /// <summary>
        /// 端口号
        /// </summary>
        public int? Port { get; set; }
        /// <summary>
        /// 允许自动重连
        /// </summary>
        public bool? AutomaticRecoveryEnabled { get; set; }
        /// <summary>
        /// 虚拟路径
        /// </summary>
        public string VirtualHost { get; set; }
        /// <summary>
        /// 服务端事件
        /// </summary>
        public Event[] Events { get; set; }
        /// <summary>
        /// 消费者
        /// </summary>
        public Consumer[] Consumers { get; set; }
        /// <summary>
        /// 预先读取的记录数
        /// </summary>
        public int prefetchCount { get; set; }
        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static EventBus2RMQConfig ReadFromConfig(string path)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<EventBus2RMQConfig>(ReadConfigContent(path));
        }
        /// <summary>
        /// 读取默认路径配置文件
        /// </summary>
        /// <returns></returns>
        public static EventBus2RMQConfig ReadFromDefaultConfig()
        {
            var path = GetDefaultConfigPath();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<EventBus2RMQConfig>(ReadConfigContent(path));
        }
        /// <summary>
        /// 获取默认配置文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultConfigPath()
        {
            return Path.Combine(GetApplicationPath(), "config/eventbus2rmq.json");
        }
        /// <summary>
        /// 获取日志路径
        /// </summary>
        /// <returns></returns>
        public static string GetLogFilePath()
        {
            var path= Path.Combine(GetApplicationPath(), "log/eventbus2rmq.log");
            return path;
        }
        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultLogPath()
        {
            var path = Path.Combine(GetApplicationPath(), "log\\");
            return path;
        }
        /// <summary>
        /// 获取应用基础路径
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationPath()
        {
            return System.AppDomain.CurrentDomain.BaseDirectory;
        }
        private static string ReadConfigContent(string path)
        {
            return System.IO.File.ReadAllText(path);
        }
    }
    public class Event
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// 订阅方式
        /// </summary>
        public string ExchangeType { get; set; }
    }
    /// <summary>
    /// 消费者
    /// </summary>
    public class Consumer
    {
        /// <summary>
        /// 接收的事件
        /// </summary>
        public string EventName { get; set; }
        /// <summary>
        /// 消费者名称
        /// </summary>
        public string ConsumerName { get; set; }
        /// <summary>
        /// 处理线程
        /// </summary>
        public string EventProcesser { get; set; }
        /// <summary>
        /// 在服务器重启时，能够存活
        /// </summary>
        public bool? Durable { get; set; }
        /// <summary>
        /// 是否为当前连接的专用队列，在连接断开后，会自动删除该队列
        /// </summary>
        public bool? Exclusive { get; set; }
        /// <summary>
        /// 当没有任何消费者使用时，自动删除该队列
        /// </summary>
        public bool? AutoDelete { get; set; }
    }
}
