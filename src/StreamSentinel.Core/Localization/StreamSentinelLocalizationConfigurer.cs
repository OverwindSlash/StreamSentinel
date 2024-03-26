using Abp.Configuration.Startup;
using Abp.Localization.Dictionaries;
using Abp.Localization.Dictionaries.Xml;
using Abp.Reflection.Extensions;

namespace StreamSentinel.Localization
{
    public static class StreamSentinelLocalizationConfigurer
    {
        public static void Configure(ILocalizationConfiguration localizationConfiguration)
        {
            localizationConfiguration.Sources.Add(
                new DictionaryBasedLocalizationSource(StreamSentinelConsts.LocalizationSourceName,
                    new XmlEmbeddedFileLocalizationDictionaryProvider(
                        typeof(StreamSentinelLocalizationConfigurer).GetAssembly(),
                        "StreamSentinel.Localization.SourceFiles"
                    )
                )
            );
        }
    }
}
