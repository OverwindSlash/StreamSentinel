using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Entities.Events
{
    public class FrameExpiredEvent : EventBase
    {
        private readonly long _frameId;

        public long FrameId => _frameId;

        public FrameExpiredEvent(long frameId)
        {
            _frameId = frameId;
        }

        public FrameExpiredEvent(Frame frame)
        {
            _frameId = frame.FrameId;
        }
    }
}
