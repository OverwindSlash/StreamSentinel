using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using StreamSentinel.EntityFrameworkCore;
using StreamSentinel.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace StreamSentinel.Web.Tests
{
    [DependsOn(
        typeof(StreamSentinelWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class StreamSentinelWebTestModule : AbpModule
    {
        public StreamSentinelWebTestModule(StreamSentinelEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(StreamSentinelWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(StreamSentinelWebMvcModule).Assembly);
        }
    }
}