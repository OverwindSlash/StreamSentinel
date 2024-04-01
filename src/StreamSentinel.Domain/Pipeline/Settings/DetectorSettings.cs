namespace StreamSentinel.Pipeline.Settings
{
    public class DetectorSettings
    {
        public string AssemblyFile { get; set; }
        public string FullQualifiedClassName { get; set; }
        public string[] Parameters { get; set; }
        public string ModelPath { get; set; }
        public bool UseCuda { get; set; }
        public float Thresh { get; set; }
    }
}
