using CameraManager.RTSP;
using OpenCvSharp;
using RTSP.RawFramesDecoding;
using RTSP.RawFramesDecoding.DecodedFrames;
using RtspClientSharpCore;
using StreamSentinel.Components.Interfaces.MediaLoader;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.MediaLoader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MediaLoader.Rtsp
{
    public class RtspVideoLoader : IVideoLoader, IDisposable
    {
        private const string RtspPrefix = "rtsp://";
        private const string HttpPrefix = "http://";

        private readonly RtspVideoProcessingBase rtspVideoProcessingBase;
        private TransformParameters _transformParameters;
        private ConnectionParameters connectionParameters;

        private int _width = 1920;
        private int _height = 1080;
        private string _userName;
        private string _password;

        private string _deviceId;
        private bool _isPlaying;
        private bool _isOpen = false;

        private VideoSpecs _videoSpecs;
        private readonly IConcurrentBoundedQueue<Frame> _frameBuffer;
        private int _frameNumber = 0;
        private int _skipFrame = 8;

        public RtspVideoLoader(string deviceId, int bufferSize)
        {
            _deviceId = deviceId;
            _videoSpecs = new VideoSpecs(string.Empty, 0, 0, 0, 0);
            _frameBuffer = new ConcurrentBoundedQueue<Frame>(bufferSize);
            _isPlaying = false;

            _transformParameters = new TransformParameters(RectangleF.Empty,
                    new System.Drawing.Size(_width, _height),
                    ScalingPolicy.Stretch, RTSP.RawFramesDecoding.PixelFormat.Bgr24, ScalingQuality.FastBilinear);

            rtspVideoProcessingBase = new RtspVideoProcessingBase();
            rtspVideoProcessingBase.FrameReceived += RtspVideoProcessingBase_FrameReceived;
        }

        private void RtspVideoProcessingBase_FrameReceived(object? sender, IDecodedVideoFrame decodedVideoFrame)
        {
            _frameNumber++;
            if (_frameNumber % _skipFrame != 0)
            {
                return;
            }
            //Trace.TraceInformation($"Frame received");
            using Mat mat = new Mat(_height,_width, MatType.CV_8UC3);
            decodedVideoFrame.TransformTo(mat.Data, (int)mat.Step(), _transformParameters);
            //Cv2.ImWrite("frame.jpg", mat);
            var frame = new Frame(_deviceId, _frameNumber, 0, mat.Clone());
            _frameBuffer.Enqueue(frame);
        }

        public VideoSpecs Specs => _videoSpecs;
        public string DeviceId => _deviceId;

        public int MediaWidth => _width;

        public int MediaHeight => _height;

        public bool IsOpened => _isOpen;

        public bool IsInPlaying => _isPlaying;

        public int BufferedFrameCount => _frameBuffer.Count;
        public int BufferedMaxOccupied => _frameBuffer.MaxOccupied;

        public void Close()
        {
            
        }

        public void Dispose()
        {
        }

        public void Open(string videoPath)
        {
            if (string.IsNullOrEmpty(videoPath))
            {
                Trace.TraceError("StreamUri is null");
                return;
            }
            string address = videoPath;

            if (!address.StartsWith(RtspPrefix) && !address.StartsWith(HttpPrefix))
                address = RtspPrefix + address;

            if (!Uri.TryCreate(address, UriKind.Absolute, out Uri deviceUri))
            {
                //MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var credential = new NetworkCredential(_userName, _password);

            connectionParameters = !string.IsNullOrEmpty(deviceUri.UserInfo) ? new ConnectionParameters(deviceUri) :
                new ConnectionParameters(deviceUri, credential);

            connectionParameters.RtpTransport = RtpTransportProtocol.TCP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(5);

            _frameBuffer.Clear();
            _isOpen = true;

        }

        public void Play(int stride = 1, bool debugMode = false, int debugFrameCount = 0)
        {
            _skipFrame = stride;
            rtspVideoProcessingBase.Start(connectionParameters);
            _isPlaying = true;
        }

        public Frame RetrieveFrame()
        {
            return _frameBuffer.Dequeue();
        }

        public async Task<Frame> RetrieveFrameAsync()
        {
            return await _frameBuffer.DequeueAsync();
        }

        public void Stop()
        {
            Thread.Sleep(500);
            _isPlaying = false;
            rtspVideoProcessingBase.Stop();

        }
    }
}
