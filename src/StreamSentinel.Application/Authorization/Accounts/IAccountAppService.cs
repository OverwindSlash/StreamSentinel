using System.Threading.Tasks;
using Abp.Application.Services;
using StreamSentinel.Authorization.Accounts.Dto;

namespace StreamSentinel.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
