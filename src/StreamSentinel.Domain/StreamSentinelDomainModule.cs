using Abp.Modules;
using Abp.Reflection.Extensions;

namespace StreamSentinel
{
    [DependsOn(typeof(StreamSentinelCoreModule))]
    public class StreamSentinelDomainModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(
                typeof(StreamSentinelDomainModule).GetAssembly());
        }
    }
}