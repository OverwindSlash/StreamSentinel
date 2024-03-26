using StreamSentinel.Debugging;

namespace StreamSentinel
{
    public class StreamSentinelConsts
    {
        public const string LocalizationSourceName = "StreamSentinel";

        public const string ConnectionStringName = "Default";

        public const bool MultiTenancyEnabled = true;


        /// <summary>
        /// Default pass phrase for SimpleStringCipher decrypt/encrypt operations
        /// </summary>
        public static readonly string DefaultPassPhrase =
            DebugHelper.IsDebug ? "gsKxGZ012HLL3MI5" : "edad2d4e7d55469a9c984dde1618afb4";
    }
}
