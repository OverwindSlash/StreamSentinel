using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Abp.Modules;
using Abp.Reflection.Extensions;
using StreamSentinel.Configuration;

namespace StreamSentinel.Web.Host.Startup
{
    [DependsOn(
       typeof(StreamSentinelWebCoreModule))]
    public class StreamSentinelWebHostModule: AbpModule
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;

        public StreamSentinelWebHostModule(IWebHostEnvironment env)
        {
            _env = env;
            _appConfiguration = env.GetAppConfiguration();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(StreamSentinelWebHostModule).GetAssembly());
        }
    }
}
