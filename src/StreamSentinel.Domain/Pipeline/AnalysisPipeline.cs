using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using StreamSentinel.Components.Interfaces.AnalysisEngine;
using StreamSentinel.Components.Interfaces.MediaLoader;
using StreamSentinel.Components.Interfaces.ObjectDetector;
using StreamSentinel.Components.Interfaces.ObjectTracker;
using StreamSentinel.Components.Interfaces.RegionManager;
using StreamSentinel.DataStructures;
using StreamSentinel.Entities.AnalysisEngine;
using StreamSentinel.Entities.Events;
using StreamSentinel.Entities.Geometric;
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
        private readonly RegionManagerSettings _regionManagerSettings;
        private readonly TrackerSettings _trackerSettings;
        private readonly List<AnalysisHandlerSettings> _analysisHandlerSettings;

        private readonly ObservableSlideWindow _slideWindow;
        private readonly VideoFrameBuffer _analyzedFrameBuffer;

        private IMediaLoader _mediaLoader;
        private IObjectDetector _objectDetector;
        private IRegionManager _regionManager;
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

            _regionManagerSettings = config.GetSection("RegionManager").Get<RegionManagerSettings>();
            var regionManager = CreateInstance<IRegionManager>(
                _regionManagerSettings.AssemblyFile, _regionManagerSettings.FullQualifiedClassName);
            _services.AddTransient<IRegionManager>(sp => regionManager);

            _trackerSettings = config.GetSection("Tracker").Get<TrackerSettings>();
            var tracker = CreateInstance<IObjectTracker>(
                _trackerSettings.AssemblyFile, _trackerSettings.FullQualifiedClassName,
                new object?[] { float.Parse(_trackerSettings.Parameters[0]), int.Parse(_trackerSettings.Parameters[1]) });
            // _trackerSettings = config.GetSection("Tracker").Get<TrackerSettings>();
            // var tracker = CreateInstance<IObjectTracker>(
            //     _trackerSettings.AssemblyFile, _trackerSettings.FullQualifiedClassName);
            _services.AddTransient<IObjectTracker>(sp => tracker);

            _analysisHandlerSettings = config.GetSection("AnalysisHandlers").Get<List<AnalysisHandlerSettings>>();
            foreach (var setting in _analysisHandlerSettings)
            {
                var handler = CreateInstance<IAnalysisHandler>(setting.AssemblyFile, setting.FullQualifiedClassName,
                    new object?[] { setting.Preferences });
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

            _regionManager = _provider.GetService<IRegionManager>();
            _regionManager.LoadAnalysisDefinition(_regionManagerSettings.Parameters[0], _mediaLoader.MediaWidth, _mediaLoader.MediaHeight);

            _objectTracker = _provider.GetService<IObjectTracker>();

            _analysisHandlers = _provider.GetServices<IAnalysisHandler>().ToList();

            var analysisTask = Task.Run(() =>
            {
                while (_mediaLoader.BufferedFrameCount != 0 || _mediaLoader.IsOpened)
                {
                    var frame = _mediaLoader.RetrieveFrame();
                    frame.AddBoundingBoxes(_objectDetector.Detect(frame.Scene, _detectorSettings.Thresh));
                    _regionManager.CalcRegionProperties(frame.DetectedObjects);
                    _objectTracker.Track(frame.Scene, frame.DetectedObjects);
                    var analyzedFrame = Analyze(frame);
                    PushAanlysisResults(analyzedFrame);
                }
            });

            var videoTask = Task.Run(() =>
            {
                _mediaLoader.Play(_mediaLoaderSettings.VideoStride);
            });

            var displayTask = Task.Run(() =>
            {
                while (!analysisTask.IsCompleted)
                {
                    DebugDisplay(_analyzedFrameBuffer.Dequeue());
                }
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
        }

        private void DebugDisplay(Frame analyzedFrame)
        {
            // Draw specified area for debug
            DrawRegion(_regionManager.AnalysisDefinition.AnalysisAreas[0], analyzedFrame.Scene, Scalar.Green);
            DrawRegion(_regionManager.AnalysisDefinition.ExcludedAreas[0], analyzedFrame.Scene, Scalar.Red);
            DrawRegion(_regionManager.AnalysisDefinition.Lanes[0], analyzedFrame.Scene, Scalar.Yellow);
            DrawRegion(_regionManager.AnalysisDefinition.Lanes[1], analyzedFrame.Scene, Scalar.Yellow);
            DrawLine(_regionManager.AnalysisDefinition.CountLines[0].Item1, analyzedFrame.Scene, Scalar.Black);

            // Debug Display
            foreach (var detectedObject in analyzedFrame.DetectedObjects)
            {
                var image = analyzedFrame.Scene;
                var bbox = detectedObject.Bbox;

                // Display box for all objects.
                image.Rectangle(new Point(bbox.X, bbox.Y), new Point(bbox.X + bbox.Width, bbox.Y + bbox.Height), Scalar.Red);

                // Display id.
                image.PutText(bbox.TrackingId.ToString(), new Point(bbox.X, bbox.Y - 20), HersheyFonts.HersheyPlain, 1.0, Scalar.Red);

                // Display lane
                image.PutText("L:" + detectedObject.LaneIndex.ToString(), new Point(bbox.X + 20, bbox.Y - 20), HersheyFonts.HersheyPlain, 1.0, Scalar.Red);
            }

            Cv2.ImShow("test", analyzedFrame.Scene.Resize(new Size(1920, 1080)));
            Cv2.WaitKey(1);
        }

        private static void DrawRegion(NormalizedPolygon region, Mat frame, Scalar color)
        {
            List<Point> points = new List<Point>();
            foreach (NormalizedPoint normalizedPoint in region.Points)
            {
                var point = new Point(normalizedPoint.OriginalX, normalizedPoint.OriginalY);
                points.Add(point);
            }

            List<IEnumerable<Point>> allPoints = new List<IEnumerable<Point>>();
            allPoints.Add(points);

            frame.Polylines(allPoints, true, color);
        }

        private static void DrawLine(NormalizedLine line, Mat frame, Scalar color)
        {
            Point start = new Point(line.Start.OriginalX, line.Start.OriginalY);
            Point stop = new Point(line.Stop.OriginalX, line.Stop.OriginalY);

            frame.Line(start, stop, color);
        }
    }
}
