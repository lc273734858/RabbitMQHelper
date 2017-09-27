using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventBus2RMQ
{
    /// <summary>
    /// 生产者
    /// </summary>
    public class Service
    {
        [Obsolete]
        static public void Start()
        {
            Producter.RegisteEvent();
        }
        static public void PushData(object data, string eventName, bool Persistent = true)
        {
            Producter.PushData(data, eventName, Persistent);
        }
        static public void PushData(string jsondata, string eventName, bool Persistent = true)
        {
            Producter.PushData(jsondata, eventName, Persistent);
        }
    }
}
