using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.PtzControl;
using StreamSentinel.Eventbus;
using System.Diagnostics;

namespace Handler.FixedDeviceService
{
    public class FixedDeviceAnalysisHandler : IAnalysisHandler, IDisposable, IEventbusHandler
    {
        public string Name => nameof(FixedDeviceAnalysisHandler);

        private IDomainEventPublisher _domainEventPublisher;
        private string _class = "Person";

        private int _currentTrackId = -1;
        private int _missedCount = 0;
        private int MaxMissedCount = 10;

        private bool _isTrackingDone = false;
        //private readonly ConcurrentBoundedQueue<int> _doneTrackingList = new ConcurrentBoundedQueue<int>(3);
        private int _lastDoneTrackingTarget = -1;
        private int MaxTrackingFrameCount = 128;
        private int _continuousTrackingCount = 0;
        private string _senderPipeline;
        private string _targetPipeline;
        public FixedDeviceAnalysisHandler(Dictionary<string, string> preferences)
        {
            _class = preferences["Class"];
            MaxMissedCount = int.Parse(preferences["MaxMissedCount"]);
            MaxTrackingFrameCount = int.Parse(preferences["MaxTrackingFrameCount"]);
            _senderPipeline = preferences["SenderPipeline"];
            _targetPipeline = preferences["TargetPipeline"];
        }

        public StreamSentinel.Entities.AnalysisEngine.AnalysisResult Analyze(StreamSentinel.Entities.AnalysisEngine.Frame frame)
        {
            if (frame.DetectedObjects.Count() <= 0)
            {
                return new AnalysisResult(true);
            }
            var targets = frame.DetectedObjects.Where(p => p.Label.Contains(_class));
            if (targets.Count() <= 0)
            {
                return new AnalysisResult(true);
            }
            
            if (!targets.Any(p => p.TrackId == _currentTrackId))
            {
                _missedCount++;
                if (_missedCount <= MaxMissedCount)
                {
                    return new AnalysisResult(true);
                }
                else
                {
                    _missedCount = 0;
                }
            }
            else
            {
                if (_continuousTrackingCount++ < MaxTrackingFrameCount)
                {
                    return new AnalysisResult(true);

                }
                else
                {
                    _continuousTrackingCount = 0;
                    //
                    _lastDoneTrackingTarget = _currentTrackId;
                }
            }
            
            

            // event response
            // 在收到通知时，编号改为次大

            // remove current object done tracking
            var current = frame.DetectedObjects.FirstOrDefault(p => p.TrackId == _lastDoneTrackingTarget);
            if (current != null)
            {
                frame.DetectedObjects.Remove(current);

            }


            var maxSeq = targets.OrderByDescending(p => p.TrackId).FirstOrDefault();
            if (maxSeq != null)
            {

                // publish to camera service
                var eventargs = new FixedEvent(nameof(FixedDeviceAnalysisHandler), new TargetBase
                {
                    BBox = maxSeq.CurrentBoundingBox,
                    Direction = DirectionEnum.Coming,
                    CommandSource = CommandSourceEnum.FixedCamera
                }, maxSeq.TrackId);

                _domainEventPublisher.PublishEvent(eventargs);
                _currentTrackId = maxSeq.TrackId;
                _missedCount = 0;

                SendLockTargetNotification(maxSeq.TrackId);
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

        public void HandleNotification(NotificationEventArgs e)
        {
            switch (e)
            {
                case PipelineSnapshotDoneEventArgs:
                    Trace.TraceInformation($"{this.Name} received PipelineSnapshotDoneEventArgs notification from {e.SenderPipeline}: {e.Message}");
                    _isTrackingDone= true;
                    //_doneTrackingList.Enqueue(_currentTrackId);
                    _lastDoneTrackingTarget = _currentTrackId;
                    _currentTrackId = -1;
                    break;
                default:
                    break;
            }
        }

        public void SendLockTargetNotification(int trackId)
        {
            var e =
                new PipelineLockTargetEventArgs($"Lock the target: {trackId}", this.Name, _senderPipeline, _targetPipeline);
            e.TrackId = trackId;
            Trace.TraceInformation($"{this.Name} send PipelineLockTargetEventArgs notification from {e.SenderPipeline}: {e.Message}");

            EventBus.Instance.PublishNotification(e);
        }
    }
}
