using StreamSentinel.Entities.Events.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamSentinel.Entities.Events.PtzControl
{
    public class PtzEvent: DomainEventBase
    {
        public ITarget Target { get; set; }
        public PtzEvent(string handlerName, ITarget target):base(nameof(PtzEvent), handlerName) 
        { 
            Target = target;
        }


    }
}
