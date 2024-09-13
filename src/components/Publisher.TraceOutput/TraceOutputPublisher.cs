using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.Events.Algorithm;
using StreamSentinel.Entities.Events.Domain;

namespace Publisher.TraceOutput
{
    public class TraceOutputPublisher : IDomainEventPublisher
    {
        public async Task<bool> PublishEvent(DomainEventBase domainEvent)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"{domainEvent.Timestamp.ToLongTimeString()} Event:{domainEvent.EventName} Message:{domainEvent.Message}");

                var @event = domainEvent as MultiOccurenceEvent;
                if (@event != null)
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string filename = @event.SnapshotId.Replace(':', '_');

                    string eventDir = $"Snapshots/Events";

                    if (!Directory.Exists(eventDir))
                    {
                        Directory.CreateDirectory(eventDir);
                    }

                    @event.Snapshot.SaveImage($"{eventDir}/{filename}_{timestamp}.jpg");
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
