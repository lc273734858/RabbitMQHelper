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
            Connection = Helper.OpenConnection(config);
        }
    }
}