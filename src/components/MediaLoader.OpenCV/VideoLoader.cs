using OpenCvSharp;
using StreamSentinel.Components.Interfaces.MediaLoader;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.MediaLoader;

namespace MediaLoader.OpenCV;

public class VideoLoader : IVideoLoader, IDisposable
{
    private string _deviceId;
    private VideoCapture _capture;
    private VideoSpecs _videoSpecs;
    private readonly IConcurrentBoundedQueue<Frame> _frameBuffer;
    private bool _isInPlaying;
    private long _index;

    public VideoLoader()
    : this("tempId", 100)
    { }

    public VideoLoader(string deviceId, int bufferSize)
    {
        _deviceId = deviceId;
        _capture = new VideoCapture();
        _videoSpecs = new VideoSpecs(string.Empty, 0, 0, 0, 0);
        _frameBuffer = new ConcurrentBoundedQueue<Frame>(bufferSize);
        _isInPlaying = false;
        _index = 0;
    }

    public string DeviceId => _deviceId;
    public bool IsOpened => _capture.IsOpened();
    public bool IsInPlaying => _isInPlaying;
    public VideoSpecs Specs => _videoSpecs;
    public int BufferedFrameCount => _frameBuffer.Count;
    public int BufferedMaxOccupied => _frameBuffer.MaxOccupied;

    public void Open(string uri)
    {
        Close();

        _capture = new VideoCapture(uri, VideoCaptureAPIs.FFMPEG);
        if (!_capture.IsOpened())
        {
            throw new Exception($"Stream source '{uri}' not available.");
        }

        _videoSpecs = new VideoSpecs(uri, _capture.FrameWidth, _capture.FrameHeight,
            _capture.Fps, _capture.FrameCount);

        _frameBuffer.Clear();
        _isInPlaying = false;
        _index = 0;
    }

    public void Close()
    {
        if (_capture != null && _capture.IsOpened())
        {
            _capture.Release();
        }
    }

    public void Play(int stride = 1, bool debugMode = false, int debugFrameCount = 0)
    {
        if (!_capture.IsOpened())
        {
            throw new Exception($"Stream source not opened.");
        }

        _isInPlaying = true;

        var startTimestamp = DateTime.Now;

        while (_isInPlaying)
        {
            #region retrive specified amount of frame for debug
            if (debugMode && debugFrameCount-- <= 0)
            {
                break;
            }
            #endregion

            _capture.Grab();
            if (_index++ % stride != 0)
            {
                continue;
            }

            var image = new Mat();
            if (!_capture.Retrieve(image))
            {
                break;
            }

            if (image.Width == 0 || image.Height == 0)
            {
                continue;
            }

            var frameId = (long)_capture.Get(VideoCaptureProperties.PosFrames);
            var offsetMilliSec = (long)_capture.Get(VideoCaptureProperties.PosMsec);

            TimeSpan elapsedTime = DateTime.Now - startTimestamp;
            var sleepMilliSec = offsetMilliSec - (long)elapsedTime.TotalMilliseconds;

            if (sleepMilliSec > 0)
            {
                Thread.Sleep((int)sleepMilliSec);
            }

            var frame = new Frame(_deviceId, frameId, offsetMilliSec, image);
            _frameBuffer.Enqueue(frame);
        }

        Close();
    }

    public void Stop()
    {
        _isInPlaying = false;
    }

    public Frame RetrieveFrame()
    {
        return _frameBuffer.Dequeue();
    }

    public async Task<Frame> RetrieveFrameAsync()
    {
        return await _frameBuffer.DequeueAsync();
    }

    public void Dispose()
    {
        Close();
        _capture.Dispose();
    }
}