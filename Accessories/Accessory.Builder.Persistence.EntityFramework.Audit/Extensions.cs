using Accessory.Builder.Core.Builders;
using Accessory.Builder.Persistence.Core.Common.Logs;
using Accessory.Persistence.EntityFramework.Audit.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Persistence.EntityFramework.Audit;

public static class Extensions
{
    public static IAccessoryBuilder AddEntityFrameworkAuditTrail<TDbContext>(this IAccessoryBuilder builder,
        string sectionName = "AuditTrail") where TDbContext : DbContext
    {
        if (!builder.TryRegisterAccessory(sectionName)) 
            return builder;
           
        builder.Services.AddScoped<IAuditTrailRepository, AuditTrailRepository<TDbContext>>();
        builder.Services.AddScoped<IAuditTrailProvider, AuditTrailProvider<TDbContext>>();
        return builder;
    }
}