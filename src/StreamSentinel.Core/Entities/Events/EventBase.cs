using System;

namespace StreamSentinel.Entities.Events
{
    public class EventBase
    {
        private readonly Guid _eventId;
        private readonly DateTime _timestamp;

        public Guid EventId => _eventId;
        public DateTime Timestamp => _timestamp;

        public EventBase()
        {
            _eventId = Guid.NewGuid();
            _timestamp = DateTime.Now;
        }
    }
}
