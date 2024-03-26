using Abp.Authorization;
using StreamSentinel.Authorization.Roles;
using StreamSentinel.Authorization.Users;

namespace StreamSentinel.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
