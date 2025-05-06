using Accessory.Builder.Core.Builders;
using Accessory.Builder.Core.Common;
using Accessory.Builder.Redis.Common;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Accessory.Builder.Redis;

public static class Extensions
{
    public static IAccessoryBuilder AddRedisCacheIntegration(this IAccessoryBuilder builder, string sectionName = "Redis")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
            
        var redisProperties = builder.GetSettings<RedisProperties>(sectionName);
        var appProperties = builder.GetSettings<AppProperties>(sectionName);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.InstanceName = appProperties.InstanceId.ToString();
            options.ConfigurationOptions = new ConfigurationOptions()
            {
                EndPoints = { $"{redisProperties.Host}:{redisProperties.Port}" },
                Ssl = redisProperties.Ssl,
                Password = redisProperties.Password
            };
        });

        return builder;
    }
}