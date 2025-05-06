using Azure.Monitor.OpenTelemetry.AspNetCore;
using Accessory.Builder.Core.Builders;
using Accessory.Builder.Core.Common;
using Accessory.Builder.Logging.OpenTelemetry.Clients;
using Accessory.Builder.Logging.OpenTelemetry.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Accessory.Builder.Logging.OpenTelemetry;

public static class Extensions
{
    public static IAccessoryBuilder AddOpenTelemetry(
        this IAccessoryBuilder builder,
        string sectionName = "Logging",
        string appName = "App")
    {
        var loggingProperties = builder.GetSettings<LoggingProperties>(sectionName);
        var appProperties = builder.GetSettings<AppProperties>(appName);
        builder.Services.AddSingleton(loggingProperties);
        builder.Services.AddSingleton(appProperties);
        var level = GetLogLevel(loggingProperties);


        builder.Services.AddSingleton<TelemetrySource>(_ => new TelemetrySource(appProperties.Name));
        builder.Services.AddSingleton<ITelemetryClient, TelemetryClient>();
        var otBuilder = builder.Services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder =>
            {
                resourceBuilder.AddService(
                    serviceName: appProperties.Name,
                    serviceVersion: appProperties.Version);
            })
            .WithTracing(providerBuilder => providerBuilder.AddSource(appProperties.Name))
            .WithMetrics(providerBuilder => providerBuilder.AddMeter(appProperties.Name));

        if (!string.IsNullOrWhiteSpace(loggingProperties.AzureMonitor.ConnectionString))
        {
            otBuilder
                .UseAzureMonitor(options =>
                {
                    options.ConnectionString = loggingProperties.AzureMonitor.ConnectionString;
                });
        }
        builder.Services.AddLogging(loggerBuilder =>
        {
            loggerBuilder.AddFilter<OpenTelemetryLoggerProvider>("*", level);
        });

        return builder;
    }

    private static LogLevel GetLogLevel(LoggingProperties loggingProperties)
    {
        if (!Enum.TryParse<LogLevel>(loggingProperties.LogLevel.Default, true, out var level))
        {
            level = LogLevel.Information;
        }

        return level;
    }
}