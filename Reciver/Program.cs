using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reciver
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                 EventBus2RMQ.ConsumerClient.RegistAndStartComsumer();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Console.ReadLine();
        }

    }
    public class Processer : EventBus2RMQ.IResumer
    {
        public bool IsDiscardErrorData => throw new NotImplementedException();

        public void ErrorHandler(Exception ex, string jsaondata)
        {
            Console.WriteLine(ex.Message);
        }

        public void ProcessData(string jsondata)
        {
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(jsondata);
        }
    }
}
