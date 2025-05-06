using System;
using System.Threading.Tasks;
using Azure.Identity;
using Accessory.Builder.Core;
using Accessory.Builder.HttpClient;
using Accessory.Builder.Logging.OpenTelemetry;
using Accessory.Builder.ServiceHealthCheck;
using Accessory.Builder.ServiceHealthCheck.Types;
using Accessory.Builder.Swagger;
using Accessory.Builder.WebApi;
using Microsoft.AspNetCore.Builder;
using TaskManager.Infrastructure;
using Microsoft.Extensions.Configuration;
using static System.String;

namespace TaskManager.Api;

class Program
{
    public static Task Main(string[] args)
        => CreateWebApplication(args).RunAsync();

    public static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Inititalize configuration settings and its source
        var connectionString = builder.Configuration["AppConfiguration:ConnectionString"];
        var appConfigEndpoint = builder.Configuration["AppConfiguration:Endpoint"];
        if (!IsNullOrEmpty(connectionString))
        {
            builder.Configuration.AddAzureAppConfiguration(options => options.Connect(connectionString));
        }
        else if (!IsNullOrEmpty(appConfigEndpoint))
        {
            builder.Configuration.AddAzureAppConfiguration(options =>
                options.Connect(new Uri(appConfigEndpoint), new ManagedIdentityCredential()));
        }

        // Add Accessory and its services
        builder.Services
            .AddAccessory(builder.Configuration)
            .AddWebApi()
            .AddApiContext()
            .AddSwagger()
            .AddOpenTelemetry()
            .AddHttpClient()
            .AddServiceHealthChecks(healthChecksBuilder => 
                healthChecksBuilder.WithServiceHealthCheck(new DatabaseServiceHealthCheck("Database")))
            .AddInfrastructure()
            .Build();

        var app = builder.Build();

        app.UseAccessory()
            .UseErrorHandler()
            .UseSwaggerDoc()
            .UseControllers()
            .UseInfrastructure();

        app.MapControllers();
        return app;
    }
}