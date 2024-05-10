using StreamSentinel.Entities.AnalysisEngine;

namespace StreamSentinel.Pipeline.Settings
{
    public class DetectorSettings:ISetting
    {
        public string AssemblyFile { get; set; }
        public string FullQualifiedClassName { get; set; }
        public string[] Parameters { get; set; }
        public string ModelPath { get; set; }
        public bool UseCuda { get; set; }
        public float Thresh { get; set; }
        public List<int> ObjType { get; set; }

        public YoloVersion YoloVersion { get; set; } = YoloVersion.Yolov5;

    }

    public enum YoloVersion : byte
    {
        Yolov5 = 0,
        YoloCustom
    }
}
