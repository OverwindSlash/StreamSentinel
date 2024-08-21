using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Algorithm;
using StreamSentinel.Entities.Events.Pipeline;

namespace Handler.EventAlg.MultiOccurrence
{
    public class MultiOccurrenceService : IAnalysisHandler, IDisposable
    {
        private ISnapshot _snapshot;
        private IDomainEventPublisher _domainEventPublisher;

        public string Name => nameof(MultiOccurrenceService);

        private readonly double _closeThreshold = 0.2;
        private string _primaryType = string.Empty;
        private List<string> _auxiliaryType = new List<string>();
        
        public MultiOccurrenceService(Dictionary<string, string> preferences)
        {
            _closeThreshold = double.Parse(preferences["CloseThreshold"]);
            _primaryType = preferences["PrimaryType"];
            _auxiliaryType = preferences["AuxiliaryType"].Split(',').ToList();
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
            for (int outterId = 0; outterId < frame.DetectedObjects.Count - 1; outterId++)
            {
                var outterObj = frame.DetectedObjects[outterId];
                if (string.Compare(outterObj.Label, _primaryType, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                for (int innerId = outterId + 1; innerId < frame.DetectedObjects.Count; innerId++)
                {
                    var innerObj = frame.DetectedObjects[innerId];

                    if (!_auxiliaryType.Contains(innerObj.Label.ToLower()))
                    {
                        continue;
                    }

                    if (outterObj.CloseTo(innerObj, _closeThreshold))
                    {
                        string combinedId = $"cb_{outterObj.Id}";
                        float score = (outterObj.Confidence + innerObj.Confidence) / 2;
                        BoundingBox bbox = outterObj.CombineBoundingBox(innerObj);
                        _snapshot.AddSnapshotOfObjectById(combinedId, score, frame, bbox);

                        var snapshots = _snapshot.GetObjectSnapshotsByObjectId(combinedId);
                        var multiOccurenceEvent = new MultiOccurenceEvent(new List<string>() { outterObj.Label, innerObj.Label}, snapshots[score]);
                        _domainEventPublisher.PublishEvent(multiOccurenceEvent);
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
