using Abp.Application.Services.Dto;

namespace StreamSentinel.Roles.Dto
{
    public class PagedRoleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}

