using Accessory.Builder.Core.Builders;
using Accessory.Builder.ServiceHealthCheck.Types;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.ServiceHealthCheck.Common;

public class AccessoryHealthChecksBuilder : IAccessoryHealthChecksBuilder
{
    private readonly List<IServiceHealthCheck> _serviceHealthChecks = new();
    private readonly IAccessoryBuilder _AccessoryBuilder;

    public AccessoryHealthChecksBuilder(IAccessoryBuilder AccessoryBuilder)
    {
        _AccessoryBuilder = AccessoryBuilder;
    }

    public IAccessoryHealthChecksBuilder WithServiceHealthCheck(IServiceHealthCheck serviceHealthCheck)
    {
        _serviceHealthChecks.Add(serviceHealthCheck);
        return this;
    }

    public IHealthChecksBuilder Build()
    {
        var healthChecksBuilder = _AccessoryBuilder.Services.AddHealthChecks();
        foreach (var serviceHealthCheck in _serviceHealthChecks)
        {
            serviceHealthCheck.Register(_AccessoryBuilder, _AccessoryBuilder.Services.AddHealthChecks());
        }

        return healthChecksBuilder;
    }
}