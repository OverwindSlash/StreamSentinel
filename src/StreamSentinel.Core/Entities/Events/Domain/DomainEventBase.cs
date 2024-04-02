using System;

namespace StreamSentinel.Entities.Events.Domain
{
    public class DomainEventBase
    {
        private readonly Guid _eventId;
        private readonly DateTime _timestamp;
        private readonly string _eventName;
        private readonly string _handlerName;

        public Guid EventId => _eventId;
        public DateTime Timestamp => _timestamp;
        public string EventName => _eventName;
        public string HandlerName => _handlerName;

        public DomainEventBase()
            : this(nameof(DomainEventBase), "Unknown") 
        { }

        public DomainEventBase(string eventName, string handlerName)
        {
            _eventId = Guid.NewGuid();
            _timestamp = DateTime.Now;
            _eventName = eventName;
            _handlerName = handlerName;
        }

        public virtual string GetDetail()
        {
            return string.Empty;
        }
    }
}
