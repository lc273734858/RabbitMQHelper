using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReciverTestProcesser
{
    public class Processer : EventBus2RMQ.IResumer
    {
        public bool IsDiscardErrorData { get { return false; } }

        public void ErrorHandler(Exception ex, string jsaondata)
        {
            Console.WriteLine(ex.Message);
        }

        public void ProcessData(string jsondata)
        {
            System.Threading.Thread.Sleep(500);
            Console.WriteLine(DateTime.Now);
            Console.WriteLine(jsondata);
            throw new Exception("test");
        }
    }
}
