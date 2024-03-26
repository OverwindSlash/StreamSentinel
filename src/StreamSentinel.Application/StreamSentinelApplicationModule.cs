using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using StreamSentinel.Authorization;

namespace StreamSentinel
{
    [DependsOn(
        typeof(StreamSentinelCoreModule),
        typeof(StreamSentinelDomainModule),
        typeof(AbpAutoMapperModule))]
    public class StreamSentinelApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<StreamSentinelAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(StreamSentinelApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
