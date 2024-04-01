namespace StreamSentinel.Pipeline.Settings
{
    public class MediaLoaderSettings
    {
        public string AssemblyFile { get; set; }
        public string FullQualifiedClassName { get; set; }
        public string[] Parameters { get; set; }
        public int VideoStride { get; set; }
    }
}
