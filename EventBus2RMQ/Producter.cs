using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus2RMQ
{
    /// <summary>
    /// 生产者
    /// </summary>
    public class Producter : BaseConfig
    {
        #region Fields
        /// <summary>
        /// 错误
        /// </summary>
        /// <param name="jsondata"></param>
        /// <param name="message"></param>
        public delegate void ErrorHandler(string jsondata, string message);
        public static JsonSerializerSettings jsonset = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore };
        #endregion

        #region Constructor

        /// <summary>
        /// 向MQ注册事件
        /// </summary>
        static public void RegisteEvent()
        {
            using (IModel channel = Connection.CreateModel())
            {
                foreach (var item in config.Events)
                {
                    channel.ExchangeDeclare(item.EventName, item.ExchangeType,true);
                }
            }
        }
        #endregion

        #region PushData
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="eventName">事件名称</param>
        /// <param name="Persistent">数据是否持久化，默认持久化</param>
        static public void PushData(object data, string eventName, bool Persistent = true)
        {
            PushDataByRoutingKey(data, eventName,"", Persistent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="eventName">事件名称</param>
        /// <param name="routing">路由</param>
        /// <param name="Persistent">是否持久化</param>
        static public void PushDataByRoutingKey(object data, string eventName, string routing, bool Persistent = true)
        {
            var jsondata = JsonConvert.SerializeObject(data, jsonset);
            PushDataByRoutingKey(jsondata, eventName,routing, Persistent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsondata"></param>
        /// <param name="eventName"></param>
        /// <param name="Persistent"></param>
        static public void PushData(string jsondata, string eventName, bool Persistent = true)
        {
            PushDataByRoutingKey(jsondata, eventName,"", Persistent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsondata"></param>
        /// <param name="eventName">事件名称</param>
        /// <param name="routing">路由</param>
        /// <param name="Persistent"></param>
        static public void PushDataByRoutingKey(string jsondata, string eventName, string routing, bool Persistent = true)
        {
            var body = Encoding.UTF8.GetBytes(jsondata);
            PushDataByRoutingKey(body, eventName, routing, Persistent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="eventName"></param>
        /// <param name="Persistent"></param>
        static public void PushData(byte[] body, string eventName, bool Persistent = true)
        {
            PushDataByRoutingKey(body, eventName, "", Persistent);
        }
        /// <summary>
        /// 根据路由发送事件
        /// </summary>
        /// <param name="body"></param>
        /// <param name="eventName"></param>
        /// <param name="routing"></param>
        /// <param name="Persistent"></param>
        static public void PushDataByRoutingKey(byte[] body, string eventName,string routing, bool Persistent = true)
        {
            try
            {
                using (IModel channel = Connection.CreateModel())
                {
                    channel.ConfirmSelect();

                    var prop = channel.CreateBasicProperties();
                    prop.Persistent = Persistent;

                    channel.BasicPublish(eventName, routing, prop, body);

                    if (!channel.WaitForConfirms())
                    {
                        throw new Exception("消息发送失败");
                    }
                }
            }
            catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
            {
                CreateConnection();
            }
        }
        #endregion

        #region PushDataAsync
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="eventName"></param>
        /// <param name="Persistent">数据是否持久化，默认持久化</param>
        static public void PushDataAsync(object data, string eventName, ErrorHandler errorCallBack, bool Persistent = true)
        {
            PushDataByRoutingAsync(data, eventName,"", errorCallBack, Persistent);
        }
        static public void PushDataByRoutingAsync(object data, string eventName,string routing, ErrorHandler errorCallBack, bool Persistent = true)
        {
            var jsondata = JsonConvert.SerializeObject(data, jsonset);
            PushDataByRoutingAsync(jsondata, eventName, routing, errorCallBack, Persistent);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsondata"></param>
        /// <param name="eventName"></param>
        /// <param name="errorCallBack"></param>
        /// <param name="Persistent"></param>
        static public void PushDataAsync(string jsondata, string eventName, ErrorHandler errorCallBack, bool Persistent = true)
        {
            PushDataByRoutingAsync(jsondata, eventName,"", errorCallBack, Persistent);
        }
        static public void PushDataByRoutingAsync(string jsondata, string eventName,string routing, ErrorHandler errorCallBack, bool Persistent = true)
        {
            var body = Encoding.UTF8.GetBytes(jsondata);
            PushDataByRoutingAsync(body, eventName, routing, errorCallBack, Persistent);
        }
        static public void PushDataAsync(byte[] body, string eventName, ErrorHandler errorCallBack, bool Persistent = true)
        {
            PushDataByRoutingAsync(body, eventName, "", errorCallBack, Persistent);

        }
        static public void PushDataByRoutingAsync(byte[] body, string eventName,string routing, ErrorHandler errorCallBack, bool Persistent = true)
        {
            var hander = new PushDataHandler((body1, eventname1, Persistent1) => {
                try
                {
                    PushDataByRoutingKey(body1, eventname1, routing, Persistent1);
                    Console.WriteLine("成功");
                    throw new Exception("推送失败");
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
                {
                    CreateConnection();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
                return string.Empty;
            });
            hander.BeginInvoke(body, eventName, Persistent, new AsyncCallback(result =>
            {
                var message = hander.EndInvoke(result);
                if (string.IsNullOrEmpty(message) == false) errorCallBack(Encoding.UTF8.GetString(body), message);
            }), null);

        }
        public delegate string PushDataHandler(byte[] body, string eventName, bool Persistent = true);
        #endregion

        #region PushDataAutoRetry
        /// <summary>
        /// 如果失败之后自动缓存，重试设置的次数后，写入本地日志
        /// </summary>
        /// <param name="data"></param>
        /// <param name="eventName">事件名称</param>
        /// <param name="times">重试次数</param>
        /// <param name="interval">重试时间间隔/毫秒</param>
        /// <param name="Persistent">数据是否持久化，默认持久化</param>
        static public void PushDataAutoRetry(object data, string eventName, int times, int interval, bool Persistent = true)
        {
            var jsondata = JsonConvert.SerializeObject(data, jsonset);
            PushDataAutoRetry(jsondata, eventName, times, interval, Persistent);
        }
        /// <summary>
        /// 如果失败之后自动缓存，重试设置的次数后，写入本地日志
        /// </summary>
        /// <param name="jsondata"></param>
        /// <param name="eventName">事件名称</param>
        /// <param name="routing"></param>
        /// <param name="times">重试次数</param>
        /// <param name="interval">重试时间间隔/毫秒</param>
        /// <param name="Persistent">数据是否持久化，默认持久化</param>
        static public void PushDataAutoRetry(string jsondata, string eventName, int times, int interval, bool Persistent = true)
        {
            var body = Encoding.UTF8.GetBytes(jsondata);
            PushDataAutoRetry(body, eventName, "",times, interval, Persistent);

        }
        /// <summary>
        /// 如果失败之后自动缓存，重试设置的次数后，写入本地日志
        /// </summary>
        /// <param name="body"></param>
        /// <param name="eventName">事件名称</param>
        /// <param name="routing"></param>
        /// <param name="times">重试次数</param>
        /// <param name="interval">重试时间间隔/毫秒</param>
        /// <param name="Persistent">数据是否持久化，默认持久化</param>
        static private void PushDataAutoRetry(byte[] body, string eventName,string routing, int times, int interval, bool Persistent = true)
        {
            for (int i = 1; i <= times; i++)
            {
                try
                {
                    using (IModel channel = Connection.CreateModel())
                    {
                        channel.ConfirmSelect();
                        var prop = channel.CreateBasicProperties();
                        prop.Persistent = Persistent;

                        channel.BasicPublish(eventName, routing, prop, body);

                        if (!channel.WaitForConfirms())
                        {
                            throw new Exception("等待确认消息超时，消息发送失败");
                        }
                        break;
                    }
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
                {
                    CreateConnection();
                }
                catch (Exception ex)
                {
                    if (i == times)
                    {
                        throw ex;
                    }
                }
                System.Threading.Thread.Sleep(interval);
            }
        }
        #endregion

        #region PushDatas
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas"></param>
        /// <param name="eventName"></param>
        /// <param name="Persistent"></param>
        /// <param name="ErrorCallBack"></param>
        static public void PushDatas<T>(ICollection<T> datas, string eventName,string routing,bool? Persistent, Action<T> ErrorCallBack)
        {
            using (IModel channel = Connection.CreateModel())
            {
                channel.ConfirmSelect();
                try
                {
                    foreach (var data in datas)
                    {
                        var bodystring = JsonConvert.SerializeObject(data, jsonset);
                        var body = Encoding.UTF8.GetBytes(bodystring);
                        var prop = channel.CreateBasicProperties();
                        prop.Persistent = Persistent.GetValueOrDefault(true);
                        channel.BasicPublish(eventName, routing, prop, body);

                        if (!channel.WaitForConfirms())
                        {
                            ErrorCallBack(data);
                        }
                    }
                }
                catch (RabbitMQ.Client.Exceptions.BrokerUnreachableException)
                {
                    CreateConnection();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        static public void PushDatas<T>(ICollection<T> datas, string eventName, bool? Persistent, Action<T> ErrorCallBack)
        {
            PushDatas(datas, eventName, "", Persistent, ErrorCallBack);
        }
        #endregion
    }
}
