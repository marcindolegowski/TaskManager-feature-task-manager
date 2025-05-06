using Accessory.Builder.Core.Builders;
using Accessory.Builder.HttpClient.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.HttpClient;

public static class Extension
{
    public static IAccessoryBuilder AddHttpClient(this IAccessoryBuilder builder, string sectionName = "httpClient")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;

        var properties = builder.GetSettings<HttpClientProperties>(sectionName);
        builder.Services.AddSingleton(properties);
        builder.Services.AddHttpClient<IHttpClient, AccessoryHttpClient>("Accessory.httpclient");
        builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        return builder;
    }
}