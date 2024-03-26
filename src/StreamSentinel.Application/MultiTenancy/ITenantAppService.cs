using Abp.Application.Services;
using StreamSentinel.MultiTenancy.Dto;

namespace StreamSentinel.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

