using System;
using RTSP.RawFramesDecoding.DecodedFrames;

namespace SimpleRtspPlayer.GUI
{
    interface IAudioSource
    {
        event EventHandler<IDecodedAudioFrame> FrameReceived;
    }
}
