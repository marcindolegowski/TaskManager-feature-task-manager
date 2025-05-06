using System;
using Accessory.Builder.Core.Builders;
using Accessory.Builder.Persistence.Core.Common;
using Accessory.Builder.Persistence.EntityFramework.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.Persistence.EntityFramework;

public static class Extensions
{
    public static IAccessoryBuilder AddEntityFramework<TDbContext>(this IAccessoryBuilder builder, string sectionName = "Database", string? subsectionName = null) where TDbContext : DbContext
    {
        if (!builder.TryRegisterAccessory(sectionName)) 
            return builder;
            
        var section = string.IsNullOrEmpty(subsectionName) ? 
            sectionName : $"{sectionName}:{subsectionName}";
            
        var properties = builder.GetSettings<PersistenceProperties>(section);
        if(properties.ConnectionString == null)
            throw new ArgumentNullException("ConnectionString is empty");
            
        builder.Services.AddSingleton(properties);
        builder.Services.AddDbContext<TDbContext>(optionsBuilder =>
        {
            optionsBuilder.UseSqlServer(properties.ConnectionString);
            optionsBuilder.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
        });
        builder.Services.AddScoped<DbContext>(p => p.GetRequiredService<TDbContext>());
            
        return builder;
    }
}