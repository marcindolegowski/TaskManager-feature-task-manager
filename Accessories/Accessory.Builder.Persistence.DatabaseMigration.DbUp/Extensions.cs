using System;
using System.IO;
using Accessory.Builder.Core.Builders;
using Accessory.Builder.Persistence.DatabaseMigration.Common;
using Accessory.Builder.Persistence.DatabaseMigration.DbUp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Accessory.Builder.Persistence.DatabaseMigration.DbUp;

public static class Extensions
{
    public static IAccessoryBuilder AddDbUp(this IServiceCollection services, Action<ILoggingBuilder> loggingConfig, string appSettingFileName = "db_appsettings.json", string sectionName = "DbUp")
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile(appSettingFileName, optional: true, reloadOnChange: true).Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging(loggingConfig);

        var builder = new AccessoryBuilder(services, config);
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;

        var dbUpProperties = builder.GetSettings<DatabaseMigrationProperties>(sectionName);
        Console.WriteLine(dbUpProperties.SchemaPath);
        Console.WriteLine(Directory.GetCurrentDirectory());
        Console.WriteLine(Path.Combine(Directory.GetCurrentDirectory(), appSettingFileName));
        builder.Services.AddSingleton(dbUpProperties);
        builder.Services.AddSingleton<IMigrationService, DbUpMigrationService>();
        builder.Services.AddSingleton<IAutoChangeService, AutoChangeService>();

        return builder;
    }
}