using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus2RMQ
{
    /// <summary>
    /// 队列方法
    /// </summary>
    public class MQInstance
    {
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
            using (IModel channel = BaseConfig.Connection.CreateModel())
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
            using (IModel channel = BaseConfig.Connection.CreateModel())
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
            using (IModel channel = BaseConfig.Connection.CreateModel())
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
        public static void PushDataToQueue(string queueName, string data, bool persistent = true)
        {
            using (IModel channel = BaseConfig.Connection.CreateModel())
            {
                var prop = channel.CreateBasicProperties();
                prop.Persistent = persistent;

                channel.BasicPublish("", queueName, prop, Encoding.UTF8.GetBytes(data));
            }
        }
        /// <summary>
        /// 批量推送数据到队列
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName">队列名称</param>
        /// <param name="datas">数据</param>
        /// <param name="errorCallBack">错误回调</param>
        /// <param name="persistent">是否持久化</param>
        public static void PushDataListToQueue<T>(string queueName, ICollection<T> datas, Action<T> errorCallBack, bool persistent = true)
        {
            using (IModel channel = BaseConfig.Connection.CreateModel())
            {
                channel.ConfirmSelect();
                try
                {
                    foreach (var data in datas)
                    {
                        var bodystring = JsonConvert.SerializeObject(data, Producter.jsonset);
                        var body = Encoding.UTF8.GetBytes(bodystring);
                        var prop = channel.CreateBasicProperties();
                        prop.Persistent = persistent;
                        channel.BasicPublish("", queueName, prop, body);

                        if (!channel.WaitForConfirms())
                        {
                            errorCallBack(data);
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        /// <summary>
        /// 采用确认机制消费队列
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="proccesser"></param>
        /// <param name="errorHandler"></param>
        public static void ConsumeQueueAck(string queueName, Action<byte[]> proccesser, Action<Exception, byte[]> errorHandler)
        {
            IModel channel = null;
            QueueingBasicConsumer consumer = null;
            channel = BaseConfig.Connection.CreateModel();
            channel.BasicQos(0, 5, false);
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, false, consumer);
            BasicDeliverEventArgs ea = null;
            while (true)
            {
                try
                {
                    ea = consumer.Queue.Dequeue();
                    proccesser(ea.Body);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    try
                    {
                        channel.BasicNack(ea.DeliveryTag, false, true);
                        errorHandler?.Invoke(ex, ea.Body);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        /// <summary>
        /// 消费队列
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="proccesser"></param>
        /// <param name="errorHandler"></param>
        public static void ConsumeQueue(string queueName, Action<byte[]> proccesser, Action<Exception, byte[]> errorHandler)
        {
            IModel channel = null;
            QueueingBasicConsumer consumer = null;
            channel = BaseConfig.Connection.CreateModel();
            channel.BasicQos(0, 5, false);
            consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(queueName, true, consumer);
            BasicDeliverEventArgs ea = null;
            while (true)
            {
                try
                {
                    ea = consumer.Queue.Dequeue();
                    proccesser(ea.Body);
                }
                catch (Exception ex)
                {
                    try
                    {
                        errorHandler?.Invoke(ex, ea.Body);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        /// <summary>
        /// 定义事件
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="eventType"></param>
        /// <param name="durable"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        public static void ExchangeDeclare(string eventName, string eventType, bool durable = true, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            using (IModel channel = BaseConfig.Connection.CreateModel())
            {
                channel.ExchangeDeclare(eventName, eventType, durable, autoDelete, arguments);
            }
        }
    }
}
