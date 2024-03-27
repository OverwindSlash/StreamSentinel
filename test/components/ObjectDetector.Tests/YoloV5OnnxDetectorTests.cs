using Detector.YoloV5Onnx;
using OpenCvSharp;
using StreamSentinel.Entities.AnalysisEngine;
using System.Diagnostics;

namespace ObjectDetector.Tests
{
    public class YoloV5OnnxDetectorTests
    {
        private string ModelPath = "Models/yolov5m.onnx";
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
        }


        [Test]
        public void TestDetectMat()
        {
            using var mat = new Mat("Images/Traffic_001.jpg", ImreadModes.Color);

            _detector.Detect(mat, 0.3F);

            var stopwatch = Stopwatch.StartNew();
            var items = _detector.Detect(mat, 0.3F);
            stopwatch.Stop();
            Console.WriteLine($"detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            //ShowResultImage(items, mat);

            Assert.That(items.Count, Is.EqualTo(17));
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
    }
}
