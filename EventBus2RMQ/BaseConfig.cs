using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EventBus2RMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class BaseConfig
    {
        /// <summary>
        /// 配置文件
        /// </summary>
        static protected EventBus2RMQConfig config = null;
        /// <summary>
        /// MQ连接
        /// </summary>
        static public IConnection Connection;
        static BaseConfig() {
            config = EventBus2RMQConfig.ReadFromDefaultConfig();
            if (config.prefetchCount == 0)
            {
                config.prefetchCount = 1;
            }
            Connection = Helper.OpenConnection(config);
        }
        public static void CreateConnection() {
            Connection = Helper.OpenConnection(config);
        }
        public static void StartService()
        {
            using (IModel channel = Connection.CreateModel())
            {

            }
        }
    }
}