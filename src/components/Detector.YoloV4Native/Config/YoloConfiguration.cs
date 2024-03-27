namespace Detector.YoloV4Native.Config
{
    public class YoloConfiguration
    {
        public string ConfigFile { get; set; }
        public string WeightsFile { get; set; }
        public string NamesFile { get; set; }

        public YoloConfiguration(string configFile, string weightsFile, string namesFile)
        {
            ConfigFile = configFile;
            WeightsFile = weightsFile;
            NamesFile = namesFile;
        }
    }
}
