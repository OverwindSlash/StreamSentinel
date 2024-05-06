using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Eventbus
{
    public class PipelineSnapshotDoneEventArgs: NotificationEventArgs
    {
        public int TrackId {  get; set; }
        public PipelineSnapshotDoneEventArgs(string message, string source, string senderPipeline, string targetPipeline)
            : base(message, source, senderPipeline, targetPipeline)
        {
        }
    }
}
