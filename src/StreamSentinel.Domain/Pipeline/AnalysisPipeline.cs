using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.MediaLoader;
using StreamSentinel.Components.Interfaces.ObjectDetector;
using StreamSentinel.Components.Interfaces.ObjectTracker;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events;
using StreamSentinel.Pipeline.Settings;
using System.Reflection;

namespace StreamSentinel.Pipeline
{
    public class AnalysisPipeline
    {
        private const int DefaultFrameLifeTime = 125;

        private readonly ServiceCollection _services;
        private readonly ServiceProvider _provider;

        private readonly PipelineSettings _pipeLineSettings;
        private readonly MediaLoaderSettings _mediaLoaderSettings;
        private readonly DetectorSettings _detectorSettings;
        private readonly TrackerSettings _trackerSettings;
        private readonly List<AnalysisHandlerSettings> _analysisHandlerSettings;

        private readonly ObservableSlideWindow _slideWindow;
        private readonly VideoFrameBuffer _analyzedFrameBuffer;

        private IMediaLoader _mediaLoader;
        private IObjectDetector _objectDetector;
        private IObjectTracker _objectTracker;
        private List<IAnalysisHandler> _analysisHandlers;
        

        public AnalysisPipeline(IConfiguration config)
        {
            _pipeLineSettings = config.GetSection("Pipeline").Get<PipelineSettings>();

            _slideWindow = new ObservableSlideWindow(DefaultFrameLifeTime);
            _analyzedFrameBuffer = new VideoFrameBuffer(DefaultFrameLifeTime);

            _services = new ServiceCollection();

            _mediaLoaderSettings = config.GetSection("MediaLoader").Get<MediaLoaderSettings>();
            var mediaLoader = CreateInstance<IMediaLoader>(
                _mediaLoaderSettings.AssemblyFile, _mediaLoaderSettings.FullQualifiedClassName,
                new object?[] { _mediaLoaderSettings.Parameters[0], int.Parse(_mediaLoaderSettings.Parameters[1]) });
            _services.AddTransient<IMediaLoader>(sp => mediaLoader);

            _detectorSettings = config.GetSection("Detector").Get<DetectorSettings>();
            var detector = CreateInstance<IObjectDetector>(
                _detectorSettings.AssemblyFile, _detectorSettings.FullQualifiedClassName);
            _services.AddTransient<IObjectDetector>(sp => detector);

            _trackerSettings = config.GetSection("Tracker").Get<TrackerSettings>();
            var tracker = CreateInstance<IObjectTracker>(
                _trackerSettings.AssemblyFile, _trackerSettings.FullQualifiedClassName,
                new object?[] { float.Parse(_trackerSettings.Parameters[0]), int.Parse(_trackerSettings.Parameters[1])});
            _services.AddTransient<IObjectTracker>(sp => tracker);

            _analysisHandlerSettings = config.GetSection("AnalysisHandlers").Get<List<AnalysisHandlerSettings>>();
            foreach (var setting in _analysisHandlerSettings)
            {
                var handler = CreateInstance<IAnalysisHandler>(setting.AssemblyFile, setting.FullQualifiedClassName);
                _slideWindow.Subscribe((IObserver<FrameExpiredEvent>)handler);
                _slideWindow.Subscribe((IObserver<ObjectExpiredEvent>)handler);
                _services.AddTransient<IAnalysisHandler>(sp => handler);
            }

            _provider = _services.BuildServiceProvider();
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
            _mediaLoader.Open(_pipeLineSettings.Uri);

            _objectDetector = _provider.GetService<IObjectDetector>();
            _objectDetector.Init(new Dictionary<string, string>() {
                {"model_path", _detectorSettings.ModelPath},
                {"use_cuda", _detectorSettings.UseCuda.ToString()}
            });

            _objectTracker = _provider.GetService<IObjectTracker>();

            _analysisHandlers = _provider.GetServices<IAnalysisHandler>().ToList();

            var analysisTask = Task.Run(() =>
            {
                while (_mediaLoader.BufferedFrameCount != 0 || _mediaLoader.IsOpened)
                {
                    var frame = _mediaLoader.RetrieveFrame();
                    frame.AddBoundingBoxes(_objectDetector.Detect(frame.Scene, _detectorSettings.Thresh));
                    _objectTracker.Track(frame.Scene, frame.DetectedObjects);
                    var analyzedFrame = Analyze(frame);
                    PushAanlysisResults(analyzedFrame);
                }
            });

            var videoTask = Task.Run(() =>
            {
                _mediaLoader.Play(_mediaLoaderSettings.VideoStride);
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

            // string detections = string.Empty;
            // var groups = analyzedFrame.DetectedObjects.GroupBy(obj => obj.Label);
            // foreach (var group in groups)
            // {
            //     detections += group.Key.ToString() + ":" + group.Count().ToString() + "; ";
            // }
            //
            // Trace.WriteLine($"FrameId: {analyzedFrame.FrameId}, {detections}");

            // Debug Display
            foreach (var detectedObject in analyzedFrame.DetectedObjects)
            {
                var image = analyzedFrame.Scene;
                var bbox = detectedObject.Bbox;

                // Display box for all objects.
                image.Rectangle(new Point(bbox.X, bbox.Y), new Point(bbox.X + bbox.Width, bbox.Y + bbox.Height), Scalar.Red);

                // Display id.
                image.PutText(bbox.TrackingId.ToString(), new Point(bbox.X, bbox.Y - 20), HersheyFonts.HersheyPlain, 1.0, Scalar.Red);
            }

            Cv2.ImShow("test", analyzedFrame.Scene.Resize(new Size(1920, 1080)));
            Cv2.WaitKey(1);
        }
    }
}
