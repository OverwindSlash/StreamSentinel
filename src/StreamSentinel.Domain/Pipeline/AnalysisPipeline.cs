using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.MediaLoader;
using StreamSentinel.Components.Interfaces.ObjectDetector;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;
using System.Diagnostics;
using System.Reflection;
using StreamSentinel.Components.Interfaces.ObjectTracker;

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
        private IObjectTracker _objectTracker;
        private List<IAnalysisHandler> _analysisHandlers;

        public AnalysisPipeline()
        {
            // TODO: get value from config file
            _settings = new PipelineSettings()
            {
                Uri = @"Video\video1.avi",
                ModelPath = @"Models\yolov5m.onnx",
                UseCuda = true
            };

            // TODO: assembly name and class name from config file
            _services = new ServiceCollection();

            var mediaLoader = CreateInstance<IMediaLoader>(
                "MediaLoader.OpenCV.dll", "MediaLoader.OpenCV.VideoLoader", 
                new object?[] { "tempId", DefaultFrameLifeTime });
            _services.AddTransient<IMediaLoader>(sp => mediaLoader);

            var detector1 = CreateInstance<IObjectDetector>(
                "Detector.YoloV5Onnx.dll", "Detector.YoloV5Onnx.YoloV5OnnxDetector");
            _services.AddTransient<IObjectDetector>(sp => detector1);

            // var detector2 = CreateInstance<IObjectDetector>(
            //     "Detector.YoloV4Native", "Detector.YoloV4Native.YoloV4NativeDetector");
            // _services.AddTransient<IObjectDetector>(sp => detector2);

            //var tracker = CreateInstance<IObjectTracker>(
            //    "Tracker.SortTracker.dll", "Tracker.SortTracker.SortTracker",
            //    new object?[] { 0.1f, 25 });
            //_services.AddTransient<IObjectTracker>(sp => tracker);

            var tracker = CreateInstance<IObjectTracker>(
                "Tracker.DeepSortTracker.dll", "Tracker.DeepSortTracker.DeepSortTracker");
            _services.AddTransient<IObjectTracker>(sp => tracker);

            _provider = _services.BuildServiceProvider();

            _slideWindow = new ObservableSlideWindow(DefaultFrameLifeTime);
            _analyzedFrameBuffer = new VideoFrameBuffer(DefaultFrameLifeTime);
        }

        private static T CreateInstance<T>(string assemblyFile, string fullQualifiedClassName, object?[] parameters = null)
        {
            Assembly assembly = Assembly.LoadFrom(assemblyFile);
            Type type = assembly.GetType(fullQualifiedClassName);

            T instance = default;
            if (parameters == null)
            {
                instance = (T)Activator.CreateInstance(type);
            }
            else
            {
                instance = (T)Activator.CreateInstance(type, parameters);
            }

            return instance;
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

            _objectTracker = _provider.GetService<IObjectTracker>();

            _analysisHandlers = new List<IAnalysisHandler>();

            var analysisTask = Task.Run(() =>
            {
                while (_mediaLoader.BufferedFrameCount != 0 || _mediaLoader.IsOpened)
                {
                    var frame = _mediaLoader.RetrieveFrame();
                    frame.AddBoundingBoxes(_objectDetector.Detect(frame.Scene));
                    _objectTracker.Track(frame.Scene, frame.DetectedObjects);
                    var analyzedFrame = Analyze(frame);
                    PushAanlysisResults(analyzedFrame);
                }
            });

            var videoTask = Task.Run(() =>
            {
                _mediaLoader.Play();
            });

            Task.WaitAll(analysisTask, videoTask);

            // Debug Display
            foreach (var frame in _analyzedFrameBuffer)
            {
                foreach (var detectedObject in frame.DetectedObjects)
                {
                    var image = frame.Scene;
                    var bbox = detectedObject.Bbox;
                
                    // Display box for all objects.
                    image.Rectangle(new Point(bbox.X, bbox.Y), new Point(bbox.X + bbox.Width, bbox.Y + bbox.Height), Scalar.Red);
                
                    // Display id.
                    image.PutText(bbox.TrackingId.ToString(), new Point(bbox.X, bbox.Y - 20), HersheyFonts.HersheyPlain, 1.0, Scalar.White);
                }
                
                Cv2.ImShow("test", frame.Scene);
                Cv2.WaitKey(20);
            }
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
