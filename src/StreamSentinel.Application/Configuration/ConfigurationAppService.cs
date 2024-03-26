using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using StreamSentinel.Configuration.Dto;

namespace StreamSentinel.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : StreamSentinelAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
