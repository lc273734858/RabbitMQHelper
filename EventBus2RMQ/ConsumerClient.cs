using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EventBus2RMQ
{
    /// <summary>
    /// 
    /// </summary>
    public class ConsumerClient : BaseConfig
    {
        /// <summary>
        /// 注册并开始消费
        /// </summary>
        public static void Start()
        {
            using (IModel channel = Connection.CreateModel())
            {
                foreach (var item in config.Consumers)
                {
                    string queue_name = channel.QueueDeclare(item.ConsumerName, item.Durable.GetValueOrDefault(true), item.Exclusive.GetValueOrDefault(false), item.AutoDelete.GetValueOrDefault(false), null);
                    channel.QueueBind(queue_name, item.EventName, "");
                }
            }
            foreach (var item in config.Consumers)
            {
                BeginConsumer(item);
            }
        }

        /// <summary>
        /// 任务
        /// </summary>
        private static List<Task> tasks = new List<Task>();
        private static void BeginConsumer(Consumer consumer)
        {
            tasks.Add(Task.Factory.StartNew(Consume, consumer, TaskCreationOptions.None));
            Console.WriteLine("开始监听"+ consumer.EventName);
        }
        private static void Consume(object state)
        {
            var config = (Consumer)state;
            IResumer process = null;
            try
            {
                process = (IResumer)Activator.CreateInstance(Type.GetType(config.EventProcesser));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            IModel channel = null;
            QueueingBasicConsumer consumer = null;
            try
            {

                channel = Connection.CreateModel();
                consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(config.ConsumerName, false, consumer);
            }
            catch (Exception ex)
            {
                process.ErrorHandler(ex, "");
                return;
            }
            string message = string.Empty;
            while (true)
            {
                BasicDeliverEventArgs ea = null;
                try
                {
                    message = string.Empty;
                    ea = consumer.Queue.Dequeue();
                    message = Encoding.UTF8.GetString(ea.Body);
                    process.ProcessData(message);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    try
                    {
                        if (ea != null)
                        {
                            if (process.IsDiscardErrorData)
                            {
                                channel.BasicAck(ea.DeliveryTag, false);
                            }
                            else
                            {
                                channel.BasicNack(ea.DeliveryTag, false, true);
                            }
                        }
                        process.ErrorHandler(ex, message);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }


        /// <summary>
        /// 停止消费
        /// </summary>
        public static void Stop()
        {
            foreach (var task in tasks)
            {
                task.Dispose();
            }
        }
    }
}
