using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.MediaLoader;
using StreamSentinel.Components.Interfaces.ObjectDetector;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Pipeline
{
    public class AnalysisPipeline
    {
        private const int DefaultFrameLifeTime = 125;
        private readonly ServiceCollection _services;
        private readonly ServiceProvider _provider;
        private readonly PipelineSettings _settings;
        private readonly ObservableSlideWindow _slideWindow;
        private readonly VideoFrameBuffer _analyzedFrameBuffer;

        private IMediaLoader _mediaLoader;
        private IObjectDetector _objectDetector;
        private List<IAnalysisHandler> _analysisHandlers;
        
        

        public AnalysisPipeline()
        {
            _settings = new PipelineSettings()
            {
                Uri = @"Video\video1.avi",
                ModelPath = @"Models\yolov5m.onnx",
                UseCuda = true
            };

            _services = new ServiceCollection();

            var loaderInstance = Activator.CreateInstance("MediaLoader.OpenCV", "MediaLoader.OpenCV.VideoLoader");
            var mediaLoader = (IMediaLoader)loaderInstance?.Unwrap();
            _services.AddTransient<IMediaLoader>(sp => mediaLoader);


            var detectorInstance1 = Activator.CreateInstance("Detector.YoloV5Onnx", "Detector.YoloV5Onnx.YoloV5OnnxDetector");
            var objectDetector1 = (IObjectDetector)detectorInstance1?.Unwrap();
            _services.AddTransient<IObjectDetector>(sp => objectDetector1);

            // var detectorInstance2 = Activator.CreateInstance("Detector.YoloV4Native", "Detector.YoloV4Native.YoloV4NativeDetector");
            // var objectDetector2 = (IObjectDetector)detectorInstance2?.Unwrap();
            // _services.AddTransient<IObjectDetector>(sp => objectDetector2);


            _provider = _services.BuildServiceProvider();

            _slideWindow = new ObservableSlideWindow(DefaultFrameLifeTime);
            _analyzedFrameBuffer = new VideoFrameBuffer(DefaultFrameLifeTime);
        }

        public void Run()
        {
            _mediaLoader = _provider.GetService<IMediaLoader>();
            _mediaLoader.Open(_settings.Uri);

            _objectDetector = _provider.GetService<IObjectDetector>();
            _objectDetector.Init(new Dictionary<string, string>() {
                {"model_path", _settings.ModelPath},
                {"use_cuda", _settings.UseCuda.ToString()}
            });

            _analysisHandlers = new List<IAnalysisHandler>();

            var analysisTask = Task.Run(() =>
            {
                while (_mediaLoader.BufferedFrameCount != 0 || _mediaLoader.IsOpened)
                {
                    var frame = _mediaLoader.RetrieveFrame();
                    frame.AddBoundingBoxes(_objectDetector.Detect(frame.Scene));
                    var analyzedFrame = Analyze(frame);
                    PushAanlysisResults(analyzedFrame);
                }
            });

            var videoTask = Task.Run(() =>
            {
                _mediaLoader.Play();
            });

            Task.WaitAll(analysisTask, videoTask);
        }

        private Frame Analyze(Frame frame)
        {
            foreach (IAnalysisHandler handler in _analysisHandlers)
            {
                var analysisResult = handler.Analyze(frame);

                // TODO with result.
            }

            _slideWindow.AddNewFrame(frame);

            return frame;
        }

        private void PushAanlysisResults(Frame analyzedFrame)
        {
            _analyzedFrameBuffer.Enqueue(analyzedFrame);

            string detections = string.Empty;
            var groups = analyzedFrame.DetectedObjects.GroupBy(obj => obj.Label);
            foreach (var group in groups)
            {
                detections += group.Key.ToString() + ":" + group.Count().ToString() + "; ";
            }

            Trace.WriteLine($"FrameId: {analyzedFrame.FrameId}, {detections}");
        }
    }
}
