using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;
using StreamSentinel.Entities.Events.PtzControl;

namespace Handler.PlateSnapshotService
{
    public class PlateSnapshotAnalysisHandler : IAnalysisHandler, IDisposable
    {
        public string Name => nameof(PlateSnapshotAnalysisHandler);

        private IDomainEventPublisher _domainEventPublisher;
        public PlateSnapshotAnalysisHandler(object placeholder)
        {

        }

        public AnalysisResult Analyze(Frame frame)
        {
            // 跟踪船只的同时将检测框放至最大，必要时缩小
            // 条件：当main pipeline 处于非控制期时可进行相对ptz控制
            // 说明：此handler 只负责跟踪，牌照抓拍由snapshot handler负责
            //       
            var minSeq = frame.DetectedObjects.OrderByDescending(p => p.TrackId).FirstOrDefault();
            if (minSeq != null)
            {
                // publish to camera service
                var eventargs = new PtzEvent(nameof(PlateSnapshotAnalysisHandler), new TargetBase
                {
                    BBox = minSeq.CurrentBoundingBox,
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
    }
}
