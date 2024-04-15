using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.PtzControl;

namespace Handler.FixedDeviceService
{
    public class FixedDeviceAnalysisHandler : IAnalysisHandler, IDisposable
    {
        public string Name => nameof(FixedDeviceAnalysisHandler);

        private IDomainEventPublisher _domainEventPublisher;
        public FixedDeviceAnalysisHandler(object placeholder)
        {
            
        }

        public StreamSentinel.Entities.AnalysisEngine.AnalysisResult Analyze(StreamSentinel.Entities.AnalysisEngine.Frame frame)
        {

            var minSeq = frame.DetectedObjects.OrderByDescending(p => p.TrackId).FirstOrDefault();
            if (minSeq != null)
            {
                // publish to camera service
                var eventargs = new FixedEvent(nameof(FixedDeviceAnalysisHandler), new TargetBase
                {
                    BBox = minSeq.CurrentBoundingBox,
                    Direction = DirectionEnum.Coming,
                    CommandSource = CommandSourceEnum.FixedCamera
                }, minSeq.TrackId);

                _domainEventPublisher.PublishEvent(eventargs);
            }

            return new AnalysisResult(true);

        }

        public void Dispose()
        {
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(StreamSentinel.Entities.Events.Pipeline.ObjectExpiredEvent value)
        {
        }

        public void OnNext(StreamSentinel.Entities.Events.Pipeline.FrameExpiredEvent value)
        {
        }

        public void SetDomainEventPublisher(IDomainEventPublisher domainEventPublisher)
        {
            _domainEventPublisher = domainEventPublisher;
        }
    }
}
