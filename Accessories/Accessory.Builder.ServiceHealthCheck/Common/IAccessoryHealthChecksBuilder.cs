using Accessory.Builder.ServiceHealthCheck.Types;

namespace Accessory.Builder.ServiceHealthCheck.Common;

public interface IAccessoryHealthChecksBuilder
{
    IAccessoryHealthChecksBuilder WithServiceHealthCheck(IServiceHealthCheck serviceHealthCheck);
}