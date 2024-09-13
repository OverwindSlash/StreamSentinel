using System.Text.Json;
using StreamSentinel.Entities.Events.Domain;

namespace Snapshot.InMemory.DomainEvents
{
    public class SnapshotsCleanedEvent : DomainEventBase
    {
        public string ObjectId { get; set; }

        public SnapshotsCleanedEvent(string message, string handlerName, string objectId)
            : base(nameof(SnapshotsCleanedEvent), message, handlerName)
        {
            ObjectId = objectId;
        }

        public override string GetDetail()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
