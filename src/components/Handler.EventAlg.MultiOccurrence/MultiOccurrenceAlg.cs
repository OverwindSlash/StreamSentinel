using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Algorithm;
using StreamSentinel.Entities.Events.Pipeline;

namespace Handler.EventAlg.MultiOccurrence
{
    public class MultiOccurrenceAlg : IAnalysisHandler, IDisposable
    {
        private ISnapshot _snapshot;
        private IDomainEventPublisher _domainEventPublisher;

        public string Name => nameof(MultiOccurrenceAlg);

        private readonly double _closeThreshold = 0.2;
        private string _primaryType = string.Empty;
        private List<string> _auxiliaryType = new List<string>();
        
        public MultiOccurrenceAlg(Dictionary<string, string> preferences)
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
            for (int primaryId = 0; primaryId < frame.DetectedObjects.Count; primaryId++)
            {
                var primaryObj = frame.DetectedObjects[primaryId];
                if (string.Compare(primaryObj.Label, _primaryType, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    continue;
                }

                for (int auxiliaryId = 0; auxiliaryId < frame.DetectedObjects.Count; auxiliaryId++)
                {
                    var auxiliaryObj = frame.DetectedObjects[auxiliaryId];

                    if (primaryObj == auxiliaryObj)
                    {
                        continue;
                    }

                    if (!_auxiliaryType.Contains(auxiliaryObj.Label.ToLower()))
                    {
                        continue;
                    }

                    if (primaryObj.CloseTo(auxiliaryObj, _closeThreshold))
                    {
                        string combinedId = $"cb_{primaryObj.Id}";
                        BoundingBox bbox = primaryObj.CombineBoundingBox(auxiliaryObj);
                        //float score = (outterObj.Confidence + innerObj.Confidence) / 2;
                        float score = bbox.Width;
                        _snapshot.AddSnapshotOfObjectById(combinedId, score, frame, bbox);

                        var snapshot = _snapshot.TakeSnapshot(frame, bbox);
                        var multiOccurenceEvent = new MultiOccurenceEvent(
                            new List<string>() { primaryObj.Label, auxiliaryObj.Label}, combinedId, snapshot);
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
