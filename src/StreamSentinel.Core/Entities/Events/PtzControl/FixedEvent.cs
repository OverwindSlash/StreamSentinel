using StreamSentinel.Entities.Events.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Entities.Events.PtzControl
{
    public class FixedEvent : DomainEventBase
    {
        public int TrackingId { get; set; }
        public ITarget Target { get; set; }
        public FixedEvent(string handlerName, ITarget target, int trackingId) : base(nameof(PtzEvent), handlerName)
        {
            Target = target;
            TrackingId = trackingId;
        }


    }
}
