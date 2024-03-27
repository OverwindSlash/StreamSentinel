using Detector.YoloV5Onnx.Models;
using Microsoft.ML.OnnxRuntime;
using OpenCvSharp;
using StreamSentinel.Components.Interfaces.ObjectDetector;
using StreamSentinel.Entities.AnalysisEngine;
using System.Diagnostics;

namespace Detector.YoloV5Onnx
{
    public class YoloV5OnnxDetector : IObjectDetector
    {
        private IYoloPredictor _predictor;
        private List<DetectionObjectType> _detectionEnabledTypes = new();

        public void PrepareEnv(Dictionary<string, string>? envParam = null)
        {
            
        }

        public void Init(Dictionary<string, string>? initParam = null)
        {
            if (!initParam.TryGetValue("model_path", out var modelPath))
            {
                throw new ArgumentException("initParam does not contain model_path element.");
            }

            SessionOptions option = null;
            if (initParam.TryGetValue("use_cuda", out var useCuda))
            {
                if (useCuda.ToLower() == "true")
                {
                    option = SessionOptions.MakeSessionOptionWithCudaProvider();
                }
            }

            _predictor = new YoloPredictor<Yolo640v5>(File.ReadAllBytes(modelPath), option);
        }

        public int GetClassNumber()
        {
            throw new NotImplementedException();
        }

        public List<BoundingBox> Detect(Mat image, float thresh = 0.5f)
        {
            var stopwatch = Stopwatch.StartNew();
            YoloPrediction[] detectedObjects = _predictor.Predict(
                image, thresh, new DetectionObjectType[]
                {
                    DetectionObjectType.Car,
                    DetectionObjectType.Person
                }).ToArray();

            stopwatch.Stop();
            Console.WriteLine($"real detection elapse: {stopwatch.ElapsedMilliseconds}ms");

            var boundingBoxes = new List<BoundingBox>();
            foreach (var prediction in detectedObjects)
            {
                var box = prediction.CurrentBoundingBox;

                var boundingBox = new BoundingBox()
                {
                    LabelId = (int)prediction.DetectionObjectType,
                    Label = prediction.DetectionObjectType.ToString(),
                    Confidence = prediction.Confidence,
                    X = box.X,
                    Y = box.Y,
                    Width = box.Width,
                    Height = box.Height
                };

                boundingBoxes.Add(boundingBox);
            }

            return boundingBoxes;
        }

        public List<BoundingBox> Detect(byte[] imageData, float thresh = 0.5f)
        {
            throw new NotImplementedException();
        }

        public List<BoundingBox> Detect(string imageFile, float thresh = 0.5f)
        {
            throw new NotImplementedException();
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void CleanupEnv()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
