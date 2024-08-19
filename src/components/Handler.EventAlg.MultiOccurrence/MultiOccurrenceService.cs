using OpenCvSharp;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;

namespace Handler.EventAlg.MultiOccurrence
{
    public class MultiOccurrenceService : IAnalysisHandler, IDisposable
    {
        public string Name => nameof(MultiOccurrenceService);

        private IDomainEventPublisher _domainEventPublisher;

        public MultiOccurrenceService()
        {
        }

        public void SetDomainEventPublisher(IDomainEventPublisher domainEventPublisher)
        {
            _domainEventPublisher = domainEventPublisher;
        }

        public AnalysisResult Analyze(Frame frame)
        {
            foreach (var outterObj in frame.DetectedObjects)
            {
                foreach (var innerObj in frame.DetectedObjects)
                {
                    if (outterObj == innerObj)
                    {
                        continue;
                    }


                }
            }
        }

        #region Observer Handlers
        void IObserver<ObjectExpiredEvent>.OnCompleted()
        {
            // Do nothing
        }

        void IObserver<FrameExpiredEvent>.OnError(Exception error)
        {
            // Do nothing
        }

        public void OnNext(FrameExpiredEvent value)
        {
            throw new NotImplementedException();
        }

        void IObserver<FrameExpiredEvent>.OnCompleted()
        {
            // Do nothing
        }

        void IObserver<ObjectExpiredEvent>.OnError(Exception error)
        {
            // Do nothing
        }

        public void OnNext(ObjectExpiredEvent value)
        {
            throw new NotImplementedException();
        }
        
        #endregion
        

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
