using System.Drawing;

namespace Detector.YoloV5Onnx
{
    public class YoloPrediction
    {
        public DetectionObjectType DetectionObjectType { get; private set; }
        public Rectangle CurrentBoundingBox { get; private set; }
        public float Confidence { get; private set; }

        public YoloPrediction(DetectionObjectType detectedObject, float confidence, Rectangle rectangle)
        {
            DetectionObjectType = detectedObject;
            Confidence = confidence;
            CurrentBoundingBox = rectangle;
        }
    }
}
