using StreamSentinel.Entities.Events.Domain;

namespace StreamSentinel.Components.Interfaces.EventPublisher
{
    public interface IDomainEventPublisher : IDisposable
    {
        Task<bool> PublishEvent(DomainEventBase domainEvent);
    }
}
