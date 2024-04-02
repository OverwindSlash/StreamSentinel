using StreamSentinel.Entities.Events.Domain;

namespace StreamSentinel.Components.Interfaces.EventPublisher
{
    public interface IDomainEventPublisher
    {
        Task<bool> PublishEvent(DomainEventBase domainEvent);
    }
}
