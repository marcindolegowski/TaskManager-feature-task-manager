## :chart_with_upwards_trend: Logging

Accessory provides integration with Azure Monitor using OpenTelemetry standard

## Getting Started

To use `Accessory.Builder.Logging.OpenTelemetry`, you need to add the following line in `Extension` class in the `Template.Infrastructure` project:

```
public static IAccessoryBuilder AddInfrastructure(this IAccessoryBuilder builder)
{
    ...
    builder.AddOpenTelemetry();
    ...
``` 

Provide necessary configuration in _appsettings.json_ file in _**PortsAndAdapters.Api**_ project
```
    "Logging": {
        "AzureMonitor": {`
            "ConnectionString": "<Provide app insights FULL connection string>"
        },
        "LogLevel": {
        "Default": "Debug"
        }
    },
```

## How to use?

- After necessary declaration you can use ILogger<T> in your classes and collect your log data in Azure Monitor instance
```
    public class SomeClass
    {
        private readonly ILogger<SomeClass> _logger;

        public SomeClass(ILogger<SomeClass> logger)
        {
            _logger = logger;
        }
    }
```

- Optionally you can use the following declaration to decorate all your incoming requests
```
    builder.Services.Decorate(typeof(ICommandHandler<>), typeof(OpenTelemetryLoggingCommandHandlerDecorator<>));
```
- There is a possibility to log telemetry data via Telemetry client
```
            private readonly ITelemetryClient _telemetryClient;
               
             ...
             
            _telemetryClient.TrackMetricWithDimension(
                CommandHandlerMetric.CommandRequested,
                1,
                CommandHandlerMetric.CommandNameDimension,
                operation.GetType().Name,
                userId);
```
