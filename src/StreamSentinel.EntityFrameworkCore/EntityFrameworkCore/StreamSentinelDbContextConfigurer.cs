using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace StreamSentinel.EntityFrameworkCore
{
    public static class StreamSentinelDbContextConfigurer
    {
        public static void Configure(DbContextOptionsBuilder<StreamSentinelDbContext> builder, string connectionString)
        {
            builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }

        public static void Configure(DbContextOptionsBuilder<StreamSentinelDbContext> builder, DbConnection connection)
        {
            builder.UseMySql(connection, ServerVersion.AutoDetect(connection.ConnectionString));
        }
    }
}
