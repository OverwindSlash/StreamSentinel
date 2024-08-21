using System.Collections.Generic;
using OpenCvSharp;
using StreamSentinel.Entities.Events.Domain;

namespace StreamSentinel.Entities.Events.Algorithm
{
    public class MultiOccurenceEvent : DomainEventBase
    {
        private List<string> _objTypes = new List<string>();
        private Mat _snapshot;

        public MultiOccurenceEvent(List<string> objTypes, Mat snapshot)
            : this("MultiOccurrence Event", "Unknown", objTypes, snapshot)
        {
            
        }


        public MultiOccurenceEvent(string eventName, string handlerName, List<string> objTypes, Mat snapshot) : base(eventName, handlerName)
        {
            _objTypes = objTypes;
            _snapshot = snapshot;
        }
    }
}
