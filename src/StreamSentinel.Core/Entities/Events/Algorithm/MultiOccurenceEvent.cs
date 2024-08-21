using System.Collections.Generic;
using OpenCvSharp;
using StreamSentinel.Entities.Events.Domain;

namespace StreamSentinel.Entities.Events.Algorithm
{
    public class MultiOccurenceEvent : DomainEventBase
    {
        public List<string> ObjTypes { get; private set; } = new List<string>();
        public string SnapshotId { get; private set; }
        public Mat Snapshot { get; private set; }

        public MultiOccurenceEvent(List<string> objTypes, string snapshotId, Mat snapshot)
            : this("MultiOccurrence Event", "Unknown", objTypes, snapshotId, snapshot)
        {
            
        }

        public MultiOccurenceEvent(string eventName, string handlerName, List<string> objTypes, string snapshotId, Mat snapshot) 
            : base(eventName, handlerName)
        {
            ObjTypes = objTypes;
            SnapshotId = snapshotId;
            Snapshot = snapshot;
        }
    }
}
