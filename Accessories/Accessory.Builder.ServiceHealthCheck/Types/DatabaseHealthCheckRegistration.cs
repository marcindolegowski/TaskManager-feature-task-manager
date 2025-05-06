using Accessory.Builder.Core.Builders;
using Accessory.Builder.Persistence.Core.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.ServiceHealthCheck.Types;

public class DatabaseServiceHealthCheck : IServiceHealthCheck
{
    private readonly string _serviceName;

    public DatabaseServiceHealthCheck(string serviceName)
    {
        _serviceName = serviceName;
    }

    public void Register(IAccessoryBuilder AccessoryBuilder, IHealthChecksBuilder healthChecksBuilder)
    {
        if (AccessoryBuilder is null) throw new ArgumentNullException($"{nameof(IAccessoryBuilder)}");

        var databaseProperties = AccessoryBuilder.GetSettings<PersistenceProperties>(_serviceName);

        if (databaseProperties?.ConnectionString is null)
            throw new ArgumentException(
                $"{typeof(PersistenceProperties)} could not be loaded from configuration. Please check, if section names are matching");

        healthChecksBuilder.AddSqlServer(
            databaseProperties.ConnectionString,
            name: _serviceName,
            tags: new[] { "Azure", "Database" }
        );
    }
}