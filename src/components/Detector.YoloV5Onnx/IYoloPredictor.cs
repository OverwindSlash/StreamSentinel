using System.Drawing;

namespace Detector.YoloV5Onnx
{
    public interface IYoloPredictor
    {
        public IReadOnlyList<YoloPrediction> Predict(Bitmap image, float targetConfidence, params DetectionObjectType[] targetDetectionTypes);
    }
}
