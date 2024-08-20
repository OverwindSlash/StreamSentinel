using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;

namespace Handler.EventAlg.MultiOccurrence
{
    public class MultiOccurrenceService : IAnalysisHandler, IDisposable
    {
        private ISnapshot _snapshot;
        private IDomainEventPublisher _domainEventPublisher;

        public string Name => nameof(MultiOccurrenceService);

        private double closeThreshold = 0.2;
        
        public MultiOccurrenceService(Dictionary<string, string> preferences)
        {
            closeThreshold = double.Parse(preferences["CloseThreshold"]);
        }

        public void SetSnapshot(ISnapshot snapshot)
        {
            _snapshot = snapshot;
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

                    if (outterObj.CloseTo(innerObj, closeThreshold))
                    {
                        string combinedId = $"cb_{outterObj.Id}";
                        float score = (outterObj.Confidence + innerObj.Confidence) / 2;
                        BoundingBox bbox = outterObj.CombineBoundingBox(innerObj);
                        _snapshot.AddSnapshotOfObjectById(combinedId, score, frame, bbox);
                    }
                }
            }

            return new AnalysisResult(true);
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
            // Do nothing
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
            // Do nothing
        }
        #endregion


        public void Dispose()
        {
            
        }
    }
}
