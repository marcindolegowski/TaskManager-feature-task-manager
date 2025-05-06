using System;
using System.IO;
using System.Linq;
using Accessory.Builder.Core.Builders;
using Accessory.Builder.Swagger.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Accessory.Builder.Swagger;

public static class Extensions
{
    public static IAccessoryBuilder AddSwagger(this IAccessoryBuilder builder, string sectionName = "Swagger")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;

        var swaggerProperties = builder.GetSettings<SwaggerProperties>(sectionName);
        builder.Services.AddSingleton(swaggerProperties);

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(swaggerProperties.ApiVersion, new OpenApiInfo 
            { 
                Title = swaggerProperties.ApiName,
                Version = swaggerProperties.ApiVersion,
                Description = swaggerProperties.ApiDescription
            });
                
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var xmlPaths = assemblies
                .Select(a => Path.Combine(AppContext.BaseDirectory, $"{a.GetName().Name}.xml"))
                .Where(a => File.Exists(a))
                .ToList();

            xmlPaths.ForEach(x => c.IncludeXmlComments(x));
        });
        return builder;
    }

    public static IApplicationBuilder UseSwaggerDoc(this IApplicationBuilder builder)
    {
        var swaggerProperties = builder.ApplicationServices.GetRequiredService<SwaggerProperties>();
        builder.UseSwagger();
        builder.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint(
                $"/swagger/{swaggerProperties.ApiVersion}/swagger.json", 
                $"{swaggerProperties.ApiName} {swaggerProperties.ApiVersion}");
            c.RoutePrefix = string.Empty;
        });
        return builder;
    }
}