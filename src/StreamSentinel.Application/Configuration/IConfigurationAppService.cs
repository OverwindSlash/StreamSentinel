using System.Threading.Tasks;
using StreamSentinel.Configuration.Dto;

namespace StreamSentinel.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
