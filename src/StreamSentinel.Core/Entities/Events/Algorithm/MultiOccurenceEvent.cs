using OpenCvSharp;
using StreamSentinel.Entities.Events.Domain;
using System.Collections.Generic;

namespace StreamSentinel.Entities.Events.Algorithm
{
    public class MultiOccurenceEvent : DomainEventBase
    {
        public List<string> ObjTypes { get; private set; } = new List<string>();
        public string SnapshotId { get; private set; }
        public Mat Snapshot { get; private set; }

        public MultiOccurenceEvent(List<string> objTypes, string snapshotId, Mat snapshot)
            : this("MultiOccurrence Event", "Unknown", "Unknown", objTypes, snapshotId, snapshot)
        {
            
        }

        public MultiOccurenceEvent(string eventName, string eventMessage, string handlerName, List<string> objTypes, string snapshotId, Mat snapshot) 
            : base(eventName, eventMessage, handlerName)
        {
            ObjTypes = objTypes;
            SnapshotId = snapshotId;
            Snapshot = snapshot;
        }
    }
}
