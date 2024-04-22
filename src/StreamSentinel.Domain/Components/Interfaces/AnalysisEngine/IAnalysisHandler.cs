using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;
using StreamSentinel.Eventbus;

namespace StreamSentinel.Components.Interfaces.AnalysisEngine
{
    public interface IAnalysisHandler : IObserver<ObjectExpiredEvent>, IObserver<FrameExpiredEvent>
    {
        public string Name { get; }

        void SetDomainEventPublisher(IDomainEventPublisher domainEventPublisher);
        AnalysisResult Analyze(Frame frame);
    }
}
