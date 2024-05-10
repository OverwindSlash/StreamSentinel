using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Detector.YoloV5Onnx.Models
{
    public class Yolov5Custom : IYoloModel
    {
        public int Width { get; } = 640;
        public int Height { get; } = 640;
        public int Depth { get; } = 3;
        public int Dimensions { get; } = 8;
        public int[] Strides { get; } = new int[] { 8, 16, 32 };

        public int[][][] Anchors { get; } = new int[][][]
        {
            new int[][] { new int[] { 010, 13 }, new int[] { 016, 030 }, new int[] { 033, 023 } },
            new int[][] { new int[] { 030, 61 }, new int[] { 062, 045 }, new int[] { 059, 119 } },
            new int[][] { new int[] { 116, 90 }, new int[] { 156, 198 }, new int[] { 373, 326 } }
        };

        public int[] Shapes { get; } = new int[] { 80, 40, 20 };
        public float Confidence { get; } = 0.20f;
        public float MulConfidence { get; } = 0.25f;
        public float Overlap { get; } = 0.45f;
        public int Channels { get; } = 3;
        public int BatchSize { get; } = 1;
        public string[] Outputs { get; } = new[] { "output" };
        public string Input { get; } = "images";
    }
}
