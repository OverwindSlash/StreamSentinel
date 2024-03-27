namespace Detector.YoloV5Onnx.Models
{
    public interface IYoloModel
    {
        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }
        public int Dimensions { get; }
        public int[] Strides { get; }
        public int[][][] Anchors { get; }
        public int[] Shapes { get; }
        public float Confidence { get; }
        public float MulConfidence { get; }
        public float Overlap { get; }
        public int Channels { get; }
        public int BatchSize { get; }
        public string[] Outputs { get; }
        public string Input { get; }
    }
}
