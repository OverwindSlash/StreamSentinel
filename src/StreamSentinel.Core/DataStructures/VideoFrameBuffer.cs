using StreamSentinel.Entities.AnalysisEngine;
using System;

namespace StreamSentinel.DataStructures
{
    public class VideoFrameBuffer : ConcurrentBoundedQueue<Frame>
    {
        public VideoFrameBuffer(int bufferSize) 
            : base(bufferSize)
        {
            
        }

        protected override void CleanupItem(Frame expiredFrame)
        {
            expiredFrame.Dispose();
        }
    }
}
