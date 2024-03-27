using OpenCvSharp;

namespace Detector.YoloV5Onnx
{
    public interface IYoloPredictor
    {
        public IReadOnlyList<YoloPrediction> Predict(Mat image, float targetConfidence, params DetectionObjectType[] targetDetectionTypes);
    }
}
