using Service.CameraManager.Service;
using StreamSentinel.Components.Interfaces.EventPublisher;
using StreamSentinel.Entities.Events.Domain;
using StreamSentinel.Entities.Events.PtzControl;

namespace Publisher.PtzControl
{
    public class PtzControlPublisher : IDomainEventPublisher
    {
        private readonly CameraManagementService cameraManagementService;
        private int trackingId = -1;
        private bool isUnderFixedControl = false;
        private bool isUnderPtzControl =false;
        public PtzControlPublisher(string configFile)
        {
            cameraManagementService = new CameraManagementService(configFile);
        }
        public Task<bool> PublishEvent(DomainEventBase domainEvent)
        {
            if (domainEvent is FixedEvent)
            {
                var fixedEvent = domainEvent as FixedEvent;
                if (fixedEvent != null)
                {
                    if (trackingId == fixedEvent.TrackingId)
                    {
                        return Task.FromResult(false);
                    }
                    trackingId = fixedEvent.TrackingId;
                    
                    ThreadPool.QueueUserWorkItem( state =>
                    {
                        while (isUnderPtzControl)
                        {
                            Task.Delay(100);
                            continue;
                        }
                        isUnderFixedControl = true;
                        cameraManagementService.LookTo(fixedEvent.Target);
                        isUnderFixedControl = false;
                        // 给pipeline发送可以拍摄的通知。考虑到延迟情况，又不能这样使用
                    });
                    
                    return Task.FromResult(true);
                }
            }
            else if (domainEvent is PtzEvent) {
                if (isUnderFixedControl)
                {
                    return Task.FromResult(false);
                }
                var ptzEvent = domainEvent as PtzEvent;
                if (ptzEvent != null)
                {
                    ThreadPool.QueueUserWorkItem(state =>
                    {
                        if (isUnderFixedControl)
                        {
                            return;
                        }
                        isUnderPtzControl=true;
                        cameraManagementService.LookTo(ptzEvent.Target);
                        isUnderPtzControl = false;
                    });
                    return Task.FromResult(true);
                }
            }
            
            return Task.FromResult(false);
        }

        public int GetTrackingId() { return trackingId;}
    }
}
