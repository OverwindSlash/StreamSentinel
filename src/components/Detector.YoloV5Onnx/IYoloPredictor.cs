using System.Drawing;
using Microsoft.ML.OnnxRuntime;

namespace Detector.YoloV5Onnx
{
    public interface IYoloPredictor
    {
        public IReadOnlyList<YoloPrediction> Predict(Bitmap image, float targetConfidence, params DetectionObjectType[] targetDetectionTypes);

        public ModelMetadata Metadata { get; }
    }
}
