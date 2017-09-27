using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EventBus2RMQ
{
    /// <summary>
    /// 消费者
    /// </summary>
    public interface IResumer
    {
        /// <summary>
        /// 错误的数据是否丢弃
        /// </summary>
        bool IsDiscardErrorData { get; }
        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="jsondata"></param>
        void ProcessData(string jsondata);
        /// <summary>
        /// 错误处理
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="jsondata">正在处理的数据，有可能为空</param>
        void ErrorHandler(Exception ex,string jsondata);
    }
}
