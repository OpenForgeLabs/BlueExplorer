using AZRedis.Domain.Models;
using StackExchange.Redis;

namespace AZRedis.Infrastructure;

internal static class RedisClientFactory
{
    public static ConfigurationOptions BuildOptions(RedisConnection connection)
    {
        if (!string.IsNullOrWhiteSpace(connection.ConnectionString))
        {
            ConfigurationOptions options = ConfigurationOptions.Parse(connection.ConnectionString, true);
            if (connection.UseTls)
                options.Ssl = true;
            return options;
        }

        var config = new ConfigurationOptions
        {
            Ssl = connection.UseTls
        };

        config.EndPoints.Add(connection.Host, connection.Port);
        if (!string.IsNullOrWhiteSpace(connection.Password))
            config.Password = connection.Password;

        return config;
    }
}
