using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.Events.Domain;
using System.Diagnostics;
using StreamSentinel.Entities.Events.Algorithm;

namespace Publisher.TraceOutput
{
    public class TraceOutputPublisher : IDomainEventPublisher
    {
        public async Task<bool> PublishEvent(DomainEventBase domainEvent)
        {
            await Task.Run(() =>
            {
                Trace.WriteLine($"{domainEvent.Timestamp.ToLongTimeString()} {domainEvent.EventName} {domainEvent.GetDetail()}");

                var @event = domainEvent as MultiOccurenceEvent;
                if (@event != null)
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string filename = @event.SnapshotId.Replace(':', '_');
                    @event.Snapshot.SaveImage($"Snapshots/Events/{filename}_{timestamp}.jpg");
                }
            });
            
            return true;
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }
    }
}
