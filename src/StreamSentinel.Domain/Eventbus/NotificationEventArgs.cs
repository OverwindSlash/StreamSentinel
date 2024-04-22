using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Eventbus
{
    public class NotificationEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string SenderPipeline { get; set; }
        public string TargetPipeline { get; set; }
        public string Source {  get; set; }

        public NotificationEventArgs(string message,string source, string senderPipeline, string targetPipeline)
        {
            Message = message;
            SenderPipeline = senderPipeline;
            TargetPipeline = targetPipeline;
            Source = source;
        }
    }
}
