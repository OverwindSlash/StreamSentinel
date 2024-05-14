using RTSP.RawFramesDecoding.DecodedFrames;
using RTSP.RawFramesReceiving;
using RtspClientSharpCore;
using SimpleRtspPlayer.GUI;
using System;


namespace CameraManager.RTSP
{
    internal class RtspVideoProcessingBase
    {
        private IRawFramesSource _rawFramesSource;
        private readonly RealtimeVideoSource _realtimeVideoSource = new RealtimeVideoSource();

        public event EventHandler<IDecodedVideoFrame> FrameReceived;
        public void Start(ConnectionParameters connectionParameters)
        {
            if (_rawFramesSource != null)
                return;

            _rawFramesSource = new RawFramesSource(connectionParameters);
            _realtimeVideoSource.SetRawFramesSource(_rawFramesSource);
            _realtimeVideoSource.FrameReceived += OnFrameReceived;

            _rawFramesSource.Start();
        }


        public void Stop()
        {
            if (_rawFramesSource == null)
                return;

            _rawFramesSource.Stop();
            _realtimeVideoSource.SetRawFramesSource(null);

            _rawFramesSource = null;
        }
        private void OnFrameReceived(object? sender, IDecodedVideoFrame e)
        {
            FrameReceived?.Invoke(sender, e);
        }
    }
}
