using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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
        public static void RegistComsumer()
        {
            using (IModel channel = Connection.CreateModel())
            {
                foreach (var item in config.Consumers)
                {
                    try
                    {
                        string queue_name = channel.QueueDeclare(item.ConsumerName, item.Durable.GetValueOrDefault(true), item.Exclusive.GetValueOrDefault(false), item.AutoDelete.GetValueOrDefault(false), null);
                        channel.QueueBind(queue_name, item.EventName, "");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        /// <summary>
        /// 注册并启动消费
        /// </summary>
        public static void RegistAndStartComsumer()
        {
            RegistComsumer();
            foreach (var item in config.Consumers)
            {
                BeginConsumer(item);
            }
        }
        private static CancellationTokenSource cts =new CancellationTokenSource();
        /// <summary>
        /// 任务
        /// </summary>
        private static List<Task> tasks = new List<Task>();
        private static void BeginConsumer(Consumer consumer)
        {
            tasks.Add(Task.Factory.StartNew(Consume, consumer, TaskCreationOptions.None));
            Console.WriteLine("开始监听"+ consumer.EventName);
        }
        private static (QueueingBasicConsumer, IModel) CreateConsumer(Consumer config, IResumer process) {
            IModel channel = null;
            QueueingBasicConsumer consumer = null;
            try
            {
                channel = Connection.CreateModel();
                channel.BasicQos(0, 5, false);
                consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(config.ConsumerName, false, consumer);
                return (consumer, channel);
            }
            catch (Exception ex)
            {
                process.ErrorHandler(ex, "");
            }
            return (null, null);

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
            (consumer,channel)= CreateConsumer(config, process);

            string message = string.Empty;
            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }
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
                        (consumer, channel) = CreateConsumer(config, process);
                    }
                    catch (NullReferenceException) {
                        (consumer, channel) = CreateConsumer(config, process);
                    }
                    catch (RabbitMQ.Client.Exceptions.ConnectFailureException)
                    {
                        (consumer, channel) = CreateConsumer(config, process);
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(ex2.ToString());
                    }
                    Thread.Sleep(500);
                }
            }
        }


        /// <summary>
        /// 停止消费
        /// </summary>
        public static void Stop()
        {
            cts.Cancel();
        }
    }
}
