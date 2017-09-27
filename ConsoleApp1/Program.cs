using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Threading;
using System.Reflection;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //TestFast();
                TestAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }
        static void Test()
        {
            var config = EventBus2RMQ.EventBus2RMQConfig.ReadFromDefaultConfig();
            //config.Events = new List<EventBus2RMQ.Event>() { new EventBus2RMQ.Event() }.ToArray();
            //config.Consumers = new List<EventBus2RMQ.Consumer>() { new EventBus2RMQ.Consumer() }.ToArray();

            //var jSetting = new JsonSerializerSettings { ContractResolver=new CamelCasePropertyNamesContractResolver()};

            //var str = JsonConvert.SerializeObject(config, jSetting);
            Console.WriteLine(config.Events[0].EventName);
            //Console.ReadLine();
            try
            {
                EventBus2RMQ.Producter.RegisteEvent();
                int indxe = 0;
                while (true)
                {                    
                    Thread.Sleep(500);
                    indxe++;
                    EventBus2RMQ.Producter.PushData(new {index= indxe, name = "1111", date = DateTime.Now }, config.Events[0].EventName,false);
                    //EventBus2RMQ.Service.PushDatas(new string[] { "1", "2", "3" }, "", true, (obj) =>
                    //{
                    //    Console.WriteLine("{0} 推送失败", obj);
                    //});
                    Console.WriteLine("{0} : push data success", DateTime.Now);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
        static void TestFast()
        {
            var config = EventBus2RMQ.EventBus2RMQConfig.ReadFromDefaultConfig();
            Console.WriteLine(config.Events[0].EventName);
            try
            {
                EventBus2RMQ.Producter.RegisteEvent();
                Int64 count = 0;
                while (true)
                {
                    //Thread.Sleep(200);
                    EventBus2RMQ.Producter.PushDataAutoRetry(new { Index=count,name = "23456", date = DateTime.Now }, config.Events[0].EventName,3,200);
                    Console.WriteLine(++count);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadLine();
            }
        }
        static void TestAsync()
        {
            try
            {
                EventBus2RMQ.Producter.RegisteEvent();
                EventBus2RMQ.Producter.PushDataAsync(new { name = "23456", date = DateTime.Now }, "TestIndex",(jsondata,error)=> { Console.WriteLine(jsondata); });
                Console.WriteLine("开始推送");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                
            }
            Console.ReadLine();
        }
    }
}
