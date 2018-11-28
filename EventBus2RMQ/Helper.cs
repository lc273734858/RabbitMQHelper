using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EventBus2RMQ
{
    public class Helper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="psw"></param>
        /// <param name="adress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        static public IConnection OpenConnection(EventBus2RMQConfig config)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.HostName = config.RabbitMQAdress;
            factory.UserName = config.UserName;
            factory.Password = config.PassWord;
            factory.VirtualHost = string.IsNullOrEmpty(config.VirtualHost)?"/": config.VirtualHost;
            //factory.Endpoint = new AmqpTcpEndpoint(config.RabbitMQAdress);
            //factory.Endpoint=new AmqpTcpEndpoint()
            if (config.Port != null)
            {
                factory.Port = config.Port.Value;
            }
            factory.AutomaticRecoveryEnabled = config.AutomaticRecoveryEnabled.GetValueOrDefault(true);
            factory.RequestedConnectionTimeout = 15000;
            return factory.CreateConnection();   //创建连接效率很低，所以一直保持一个连接  
        }
    }
}
