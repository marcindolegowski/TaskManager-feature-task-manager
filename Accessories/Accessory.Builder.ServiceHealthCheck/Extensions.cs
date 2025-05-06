using Accessory.Builder.Core.Builders;
using Accessory.Builder.ServiceHealthCheck.Common;
using Accessory.Builder.ServiceHealthCheck.Types;

namespace Accessory.Builder.ServiceHealthCheck;

public static class Extensions
{
    public static IAccessoryBuilder AddServiceHealthChecks(
        this IAccessoryBuilder AccessoryBuilder,
        Action<IAccessoryHealthChecksBuilder>? buildAction = null,
        string sectionName = "HealthChecks")
    {
        if (!AccessoryBuilder.TryRegisterAccessory(sectionName))
            return AccessoryBuilder;
        var AccessoryHealthChecksBuilder = new AccessoryHealthChecksBuilder(AccessoryBuilder);
        buildAction?.Invoke(AccessoryHealthChecksBuilder);
        AccessoryHealthChecksBuilder.Build();
        return AccessoryBuilder;
    }

    public static IAccessoryBuilder AddServiceHealthChecks(
        this IAccessoryBuilder AccessoryBuilder,
        string sectionName = "HealthChecks")
    {
        return AccessoryBuilder.AddServiceHealthChecks(
            builder => builder.WithServiceHealthCheck(new DatabaseServiceHealthCheck("Database")),
            sectionName);
    }
}