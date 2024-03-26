using System.Threading.Tasks;
using Abp.Application.Services;
using StreamSentinel.Sessions.Dto;

namespace StreamSentinel.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
