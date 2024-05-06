using Castle.Core;
using OpenCvSharp.Dnn;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events.Pipeline;
using StreamSentinel.Entities.Events.PtzControl;
using StreamSentinel.Eventbus;

namespace Handler.PlateTrackingService
{
    public class PlateTrackingAnalysisHandler : IAnalysisHandler, IDisposable
    {
        public string Name => nameof(PlateTrackingAnalysisHandler);

        private IDomainEventPublisher _domainEventPublisher;
        private string _senderPipeline = "Pipeline2";
        private string _targetPipeline = "Pipeline1";
        private string _detectType1 = "Boat";
        private string _detectType2 = "Plate";

        private int _currentTracking1 = -1;
        private int _currentTracking2 = -1;

        private int _missedFrame1 = 0;
        private int _missedFrame2 = 0;
        private int MaxMissedFrame = 16;

        private int _noDetectionFrameCount = 0;
        private int MaxNoDetectionFrameCount = 48;
        public PlateTrackingAnalysisHandler(Dictionary<string, string> preferences)
        {
            _senderPipeline = preferences["SenderPipeline"];
            _targetPipeline = preferences["TargetPipeline"];
            _detectType1 = preferences["DetectType1"];
            _detectType2 = preferences["DetectType2"];

            int.TryParse(preferences["MaxMissedFrame"], out MaxMissedFrame);
            int.TryParse(preferences["MaxNoDetectionFrameCount"], out MaxNoDetectionFrameCount);

        }
        public AnalysisResult Analyze(Frame frame)
        {
            // 跟踪船只的同时将检测框放至最大，必要时缩小
            // 条件：当main pipeline 处于非控制期时可进行相对ptz控制
            // 说明：此handler 只负责跟踪，牌照抓拍由snapshot handler负责
            //       
            if (frame.DetectedObjects.Count <= 0)
            {
                // todo: 什么也没有时 减小焦距：而且需要累积一定帧时触发
                if (_noDetectionFrameCount++ > MaxNoDetectionFrameCount)
                {
                    _noDetectionFrameCount = 0;
                    DetectedObject detectedObject = new DetectedObject();
                    detectedObject.Bbox = new BoundingBox();
                    detectedObject.Bbox.X = 0;
                    detectedObject.Bbox.Y = 0;
                    detectedObject.Bbox.Width = frame.Scene.Cols;
                    detectedObject.Bbox.Height = frame.Scene.Rows;

                    PublishTarget(detectedObject);
                }
                else
                {
                    return new AnalysisResult(true);
                }
            }

            var targets1 = frame.DetectedObjects.Where(p => p.Label.Contains(_detectType1));

            var targets2 = frame.DetectedObjects.Where(p => p.Label.Contains(_detectType2));

            if (targets1.Count() <= 0 && targets2.Count() <= 0)
            {
                // TODO: 一段时间未检测到任何目标后，需要缩小变焦倍数
                if (_noDetectionFrameCount++ >MaxNoDetectionFrameCount)
                {
                    _noDetectionFrameCount = 0;
                    DetectedObject detectedObject = new DetectedObject();
                    detectedObject.Bbox = new BoundingBox();
                    detectedObject.Bbox.X = frame.Scene.Cols/2;
                    detectedObject.Bbox.Y = frame.Scene.Rows/2;
                    detectedObject.Bbox.Width = 10;
                    detectedObject.Bbox.Height = 10;

                    PublishTarget(detectedObject);
                }
                else
                {
                    return new AnalysisResult(true);
                }
            }
            else if (targets1.Count() > 0 && targets2.Count() <= 0)
            {
                // 只有船时
                // 对准船 并适度缩放船只
                var currentTarget1 = targets1.Where(p=>p.TrackId == _currentTracking1).FirstOrDefault();
                if(currentTarget1 == null)
                {
                    if(_missedFrame1++ < MaxMissedFrame)
                    {
                        return new AnalysisResult(true);
                    }

                    
                    var boat = targets1.OrderBy(p => p.X - frame.Scene.Cols / 2).First();
                    PublishTarget(boat);
                    _currentTracking1 = boat.TrackId;
                }
                else
                {
                    PublishTarget(currentTarget1);
                }
                

            }
            else if (targets1.Count() <= 0 && targets2.Count() > 0)
            {
                // 只有牌照时
                // 对准 牌照；同时适度缩放
                var currentTarget2 = targets2.Where(p => p.TrackId == _currentTracking2).FirstOrDefault();
                if (currentTarget2 == null)
                {
                    if (_missedFrame2++ < MaxMissedFrame)
                    {
                        return new AnalysisResult(true);
                    }
                    var plate = targets2.OrderBy(p => p.X - frame.Scene.Cols / 2).First();
                    PublishTarget(plate);
                    _currentTracking2 = plate.TrackId;
                }
                else
                {
                    PublishTarget(currentTarget2);
                }
            }
            else
            {
                // 当有 牌照 和 船时
                // 对准 牌照；同时适度缩放船只
                var currentTarget2 = targets2.Where(p => p.TrackId == _currentTracking2).FirstOrDefault();
                if (currentTarget2 == null)
                {
                    if (_missedFrame2++ < MaxMissedFrame)
                    {
                        return new AnalysisResult(true);
                    }
                    var plate = targets2.OrderBy(p => p.X - frame.Scene.Cols / 2).First();
                    PublishTarget(plate);
                    _currentTracking2 = plate.TrackId;
                }
                else
                {
                    PublishTarget(currentTarget2);
                }
            }

            //var target = targets2.OrderBy(p =>
            //{
            //    var box = p.CurrentBoundingBox;
            //    var dx = box.X - frame.Scene.Cols / 2;
            //    var dy = box.Y - frame.Scene.Rows / 2;
            //    return dx * dx + dy * dy;

            //}).First();

            //var target = frame.DetectedObjects.OrderByDescending(p => p.TrackId).FirstOrDefault();
            

            return new AnalysisResult(true);
        }

        private void PublishTarget(DetectedObject target)
        {
            if (target != null)
            {
                // publish to camera service
                var eventargs = new PtzEvent(nameof(PlateTrackingAnalysisHandler), new TargetBase
                {
                    BBox = target.CurrentBoundingBox,
                    Direction = DirectionEnum.Coming,
                    CommandSource = CommandSourceEnum.PtzCamera
                });

                _domainEventPublisher.PublishEvent(eventargs);
            }
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
