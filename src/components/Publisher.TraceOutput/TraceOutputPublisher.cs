using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.Events.Domain;
using System.Diagnostics;

namespace Publisher.TraceOutput
{
    public class TraceOutputPublisher : IDomainEventPublisher
    {
        public async Task<bool> PublishEvent(DomainEventBase domainEvent)
        {
            await Task.Run(() =>
            {
                Trace.WriteLine($"{domainEvent.Timestamp.ToLongTimeString()} {domainEvent.EventName} {domainEvent.GetDetail()}");
            });
            
            return true;
        }
    }
}
