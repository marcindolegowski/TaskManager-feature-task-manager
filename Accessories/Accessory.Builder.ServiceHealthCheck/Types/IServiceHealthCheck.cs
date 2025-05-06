using Accessory.Builder.Core.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.ServiceHealthCheck.Types;

public interface IServiceHealthCheck
{
    void Register(IAccessoryBuilder AccessoryBuilder, IHealthChecksBuilder healthChecksBuilder);
}