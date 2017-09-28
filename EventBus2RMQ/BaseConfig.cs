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
        /// <summary>
        /// 订阅事件
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="consumerName">消费者名称</param>
        /// <param name="routekey">路由</param>
        /// <param name="durable">持久</param>
        /// <param name="exclusive"></param>
        /// <param name="autoDelete">自动删除</param>
        public static void Subscription(string eventName, string consumerName, string routekey = "", bool durable = true, bool exclusive = false, bool autoDelete = false)
        {
            using (IModel channel = Connection.CreateModel())
            {
                string queue_name = channel.QueueDeclare(consumerName, durable, exclusive, autoDelete, null);
                channel.QueueBind(queue_name, eventName, routekey);
            }
        }
        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="consumerName">消费者名称</param>
        ///  /// <param name="routekey">路由</param>
        public static void UnSubscription(string eventName, string consumerName, string routekey = "")
        {
            using (IModel channel = Connection.CreateModel())
            {
                channel.QueueUnbind(consumerName, eventName, routekey, null);
            }
        }
        /// <summary>
        /// 定义队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="durable">持久</param>
        /// <param name="exclusive"></param>
        /// <param name="autoDelete">自动删除</param>
        /// <returns></returns>
        public static string QueueDeclare(string queueName, bool durable = true, bool exclusive = false, bool autoDelete = false)
        {
            using (IModel channel = EventBus2RMQ.Producter.Connection.CreateModel())
            {
                return channel.QueueDeclare(queueName, durable, exclusive, autoDelete, null);
            }
        }
        /// <summary>
        /// 推送消息到队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="data">数据</param>
        /// <param name="persistent">是否持久</param>
        public static void PushDataToQueue(string queueName, string data,bool persistent=true)
        {
            using (IModel channel = Connection.CreateModel())
            {
                var prop = channel.CreateBasicProperties();
                prop.Persistent = persistent;

                channel.BasicPublish("", queueName, prop, Encoding.UTF8.GetBytes(data));
            }
        }
    }
}