using Detector.YoloV5Onnx;
using OpenCvSharp;
using StreamSentinel.Entities.AnalysisEngine;
using System.Diagnostics;

namespace ObjectDetector.Tests
{
    public class YoloV5OnnxDetectorTests
    {
        private string ModelPath = @"Models/yolov5m.onnx";
        private readonly YoloV5OnnxDetector _detector;

        public YoloV5OnnxDetectorTests()
        {
            _detector = new YoloV5OnnxDetector();

            _detector.PrepareEnv();
            _detector.Init(new Dictionary<string, string>()
            {
                {"model_path", ModelPath},
                {"use_cuda", "true"}
            });

            // // Avoid first time-consuming call in test cases.
            // using var mat = new Mat("Images/Traffic_001.jpg", ImreadModes.Color);
            // _detector.Detect(mat, 0.3F);
        }

        [Test]
        public void TestDetectMat()
        {
            using var mat = new Mat("Images/Traffic_001.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.3F);
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(18));
        }

        private static void ShowResultImage(List<BoundingBox> items, Mat mat)
        {
            foreach (BoundingBox item in items)
            {
                mat.PutText(item.Label, new Point(item.X, item.Y), HersheyFonts.HersheyPlain, 1.0, Scalar.Aqua);
                mat.Rectangle(new Point(item.X, item.Y), new Point(item.X + item.Width, item.Y + item.Height), Scalar.Aqua);
            }

            Window.ShowImages(mat);
        }

        [Test]
        public void TestDetectMatBytes()
        {
            using var mat = new Mat("Images/Traffic_001.jpg", ImreadModes.Color);

            var imageData = mat.ToBytes();

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(imageData, 0.3F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(18));
        }

        [Test]
        public void TestDetect2KMatBytes()
        {
            using var mat = new Mat("Images/Pedestrian2K.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat.ToBytes(), 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(9));
        }

        [Test]
        public void TestDetect2KMatPtr()
        {
            using var mat = new Mat("Images/Pedestrian2K.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(9));
        }

        [Test]
        public void TestDetect4KMatBytes()
        {
            using var mat = new Mat("Images/Pedestrian4K.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat.ToBytes(), 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(9));
        }

        [Test]
        public void TestDetect4KMatPtr()
        {
            using var mat = new Mat("Images/Pedestrian4K.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.7F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(9));
        }

        [Test]
        public void TestDetectHighwayMatPtr()
        {
            using var mat = new Mat("Images/Traffic_002.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.3F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(8));
        }

        [Test]
        public void TestDetectHighwayForMotionMatPtr()
        {
            using var mat = new Mat("Images/pl_000001.jpg", ImreadModes.Color);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.3F).ToList();
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(10));
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
