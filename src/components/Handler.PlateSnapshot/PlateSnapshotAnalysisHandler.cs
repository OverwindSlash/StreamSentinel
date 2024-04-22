using OpenCvSharp.Dnn;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;
using StreamSentinel.Entities.Events.PtzControl;
using StreamSentinel.Eventbus;

namespace Handler.PlateSnapshotService
{
    public class PlateSnapshotAnalysisHandler : IAnalysisHandler, IDisposable, IEventbusSender
    {
        public string Name => nameof(PlateSnapshotAnalysisHandler);

        private IDomainEventPublisher _domainEventPublisher;
        private string _senderPipeline;
        private string _targetPipeline;

        public PlateSnapshotAnalysisHandler(Dictionary<string, string> preferences)
        {
            _senderPipeline = preferences["SenderPipeline"];
            _targetPipeline = preferences["TargetPipeline"];
        }

        public AnalysisResult Analyze(Frame frame)
        {
            // 跟踪船只的同时将检测框放至最大，必要时缩小
            // 条件：当main pipeline 处于非控制期时可进行相对ptz控制
            // 说明：此handler 只负责跟踪，牌照抓拍由snapshot handler负责
            //       
            if (frame.DetectedObjects.Count <=0)
            {
                return new AnalysisResult(true);
            }
            var targets = frame.DetectedObjects.Where(p => p.Label.Contains("TV"));
            if (targets.Count() <=0)
            {
                return new AnalysisResult(true);
            }
            var target = targets.OrderBy(p =>
            {
                var box = p.CurrentBoundingBox;
                var dx = box.X  - frame.Scene.Cols/2;
                var dy = box.Y  - frame.Scene.Rows/2;
                return dx * dx + dy * dy;

            }).First();
            //var target = frame.DetectedObjects.OrderByDescending(p => p.TrackId).FirstOrDefault();
            if (target != null)
            {
                // publish to camera service
                var eventargs = new PtzEvent(nameof(PlateSnapshotAnalysisHandler), new TargetBase
                {
                    BBox = target.CurrentBoundingBox,
                    Direction = DirectionEnum.Coming,
                    CommandSource = CommandSourceEnum.PtzCamera
                });

                _domainEventPublisher.PublishEvent(eventargs);
            }

            return new AnalysisResult(true);
        }

        void IObserver<ObjectExpiredEvent>.OnCompleted()
        {
        }

        void IObserver<FrameExpiredEvent>.OnCompleted()
        {
        }

        void IObserver<ObjectExpiredEvent>.OnError(Exception error)
        {
        }

        void IObserver<FrameExpiredEvent>.OnError(Exception error)
        {
        }

        void IObserver<ObjectExpiredEvent>.OnNext(ObjectExpiredEvent value)
        {
        }

        void IObserver<FrameExpiredEvent>.OnNext(FrameExpiredEvent value)
        {
        }

        void IAnalysisHandler.SetDomainEventPublisher(IDomainEventPublisher domainEventPublisher)
        {
            _domainEventPublisher = domainEventPublisher;
        }
        public void Dispose()
        {
        }

        public void SendNotification()
        {
            var eventArgs =
                new PipelineNotificationEventArgs("Test", this.Name, _senderPipeline, _targetPipeline);

            EventBus.Instance.PublishNotification(eventArgs);
        }
    }
}
