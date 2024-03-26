using Microsoft.EntityFrameworkCore;
using Abp.Zero.EntityFrameworkCore;
using StreamSentinel.Authorization.Roles;
using StreamSentinel.Authorization.Users;
using StreamSentinel.MultiTenancy;

namespace StreamSentinel.EntityFrameworkCore
{
    public class StreamSentinelDbContext : AbpZeroDbContext<Tenant, Role, User, StreamSentinelDbContext>
    {
        /* Define a DbSet for each entity of the application */
        
        public StreamSentinelDbContext(DbContextOptions<StreamSentinelDbContext> options)
            : base(options)
        {
        }
    }
}
