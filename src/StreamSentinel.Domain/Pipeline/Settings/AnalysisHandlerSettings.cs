namespace StreamSentinel.Pipeline.Settings
{
    public class AnalysisHandlerSettings
    {
        public string AssemblyFile { get; set; }
        public string FullQualifiedClassName { get; set; }
        public string[] Parameters { get; set; }
        public Dictionary<string, string> Preferences { get; set; }
    }
}
