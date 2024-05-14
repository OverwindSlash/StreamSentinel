using System;
using RTSP.RawFramesDecoding.DecodedFrames;

namespace SimpleRtspPlayer.GUI
{
    public interface IVideoSource
    {
        event EventHandler<IDecodedVideoFrame> FrameReceived;
    }
}