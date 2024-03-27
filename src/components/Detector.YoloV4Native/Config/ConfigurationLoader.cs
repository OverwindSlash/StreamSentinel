namespace Detector.YoloV4Native.Config
{
    public class ConfigurationLoader
    {
        public static YoloConfiguration Load(string path = ".")
        {
            var files = GetYoloFiles(path);
            var yoloConfiguration = MapFiles(files);
            var configValid = AreValidYoloFiles(yoloConfiguration);

            if (configValid)
            {
                return yoloConfiguration;
            }

            throw new FileNotFoundException("Cannot found model, check all config files available (.cfg, .weights, .names)");
        }

        private static string[] GetYoloFiles(string path)
        {
            return Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                .Where(o => o.EndsWith(".names") || o.EndsWith(".cfg") || o.EndsWith(".weights")).ToArray();
        }

        private static YoloConfiguration MapFiles(string[] files)
        {
            var configurationFile = files.Where(o => o.EndsWith(".cfg")).FirstOrDefault();
            var weightsFile = files.Where(o => o.EndsWith(".weights")).FirstOrDefault();
            var namesFile = files.Where(o => o.EndsWith(".names")).FirstOrDefault();

            return new YoloConfiguration(configurationFile, weightsFile, namesFile);
        }

        private static bool AreValidYoloFiles(YoloConfiguration config)
        {
            if (string.IsNullOrEmpty(config.ConfigFile) ||
                string.IsNullOrEmpty(config.WeightsFile) ||
                string.IsNullOrEmpty(config.NamesFile))
            {
                return false;
            }

            return true;
        }
    }
}
