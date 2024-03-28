using Detector.YoloV4Native;
using OpenCvSharp;
using StreamSentinel.Entities.AnalysisEngine;
using System.Diagnostics;

namespace ObjectDetector.Tests
{
    public class YoloV4NativeDetectorTests : IDisposable
    {
        private const string ConfigPath = @"Models/yolov4";
        private readonly YoloV4NativeDetector _detector;

        public YoloV4NativeDetectorTests()
        {
            _detector = new YoloV4NativeDetector();

            _detector.PrepareEnv();
            _detector.Init(new Dictionary<string, string>()
            {
                { "config_path", ConfigPath }
            });
        }

        public void Dispose()
        {
            _detector.Close();
            _detector.CleanupEnv();
        }

        [Test]
        public void TestInit()
        {
            Assert.That(_detector.CudaEnv.CudaExists, Is.EqualTo(true));
            Assert.That(_detector.CudaEnv.CudnnExists, Is.EqualTo(true));
        }

        [Test]
        public void TestDetectMatBytes()
        {
            using var mat = new Mat("Images/Traffic_001.jpg", ImreadModes.Color);

            var imageData = mat.ToBytes();

            _detector.Detect(imageData, 0.3F).ToList();

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(imageData, 0.3F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            Assert.That(items.Count, Is.EqualTo(18));

            //ShowResultImage(items, mat);
        }

        private static void ShowResultImage(List<BoundingBox> items, Mat mat)
        {
            foreach (BoundingBox item in items)
            {
                mat.PutText(item.Label, new Point(item.X, item.Y), HersheyFonts.HersheyPlain, 1.0, Scalar.Aqua);
                mat.Rectangle(new Point(item.X, item.Y), new Point(item.X + item.Width, item.Y + item.Height),
                    Scalar.Aqua);
            }

            Window.ShowImages(mat);
        }

        [Test]
        public void TestDetectMatPtr()
        {
            using var mat = new Mat("Images/Traffic_001.jpg", ImreadModes.Color);

            _detector.Detect(mat, 0.7F).ToList();

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            Assert.That(items.Count, Is.EqualTo(14));

            //ShowResultImage(items, mat);
        }

        [Test]
        public void TestDetect2KMatBytes()
        {
            using var mat = new Mat("Images/Pedestrian2K.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat.ToBytes(), 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            Assert.That(items.Count, Is.EqualTo(11));

            //ShowResultImage(items, mat);
        }

        [Test]
        public void TestDetect2KMatPtr()
        {
            using var mat = new Mat("Images/Pedestrian2K.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            Assert.That(items.Count, Is.EqualTo(11));

            //ShowResultImage(items, mat);
        }

        [Test]
        public void TestDetect4KMatBytes()
        {
            using var mat = new Mat("Images/Pedestrian4K.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat.ToBytes(), 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            Assert.That(items.Count, Is.EqualTo(11));

            //ShowResultImage(items, mat);
        }

        [Test]
        public void TestDetect4KMatPtr()
        {
            using var mat = new Mat("Images/Pedestrian4K.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            Assert.That(items.Count, Is.EqualTo(11));

            //ShowResultImage(items, mat);
        }

        [Test]
        public void TestDetectHighwayMatPtr()
        {
            using var mat = new Mat("Images/Traffic_002.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.6F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            Assert.That(items.Count, Is.EqualTo(10));

            //ShowResultImage(items, mat);
        }

        [Test]
        public void TestDetectHighwayForMotionMatPtr()
        {
            using var mat = new Mat("Images/pl_000001.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.6F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            Assert.That(items.Count, Is.EqualTo(8));

            //ShowResultImage(items, mat);
        }

        [Test]
        public void TestDetectBechmark()
        {
            using var mat = new Mat("Images/pl_000001.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                var items = _detector.Detect(mat, 0.6F).ToList();
            }

            stopwatch.Stop();

            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds / 10}ms");
        }
    }
}
