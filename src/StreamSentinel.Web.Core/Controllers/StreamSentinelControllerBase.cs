using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace StreamSentinel.Controllers
{
    public abstract class StreamSentinelControllerBase: AbpController
    {
        protected StreamSentinelControllerBase()
        {
            LocalizationSourceName = StreamSentinelConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
