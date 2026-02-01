using AZRedis.Application.Configuration;
using AZRedis.Domain.Models;

namespace AZRedis.Infrastructure;

public static class RedisConnectionMapper
{
    public static RedisConnection ToDomain(this RedisConnectionConfig config)
    {
        return new RedisConnection(
            config.Name,
            config.ConnectionString,
            config.Host,
            config.Port,
            config.Password,
            config.UseTls,
            config.Database);
    }
}
