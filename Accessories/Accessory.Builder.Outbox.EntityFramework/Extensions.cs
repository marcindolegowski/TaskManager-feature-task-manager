using Accessory.Builder.Core.Builders;
using Accessory.Builder.Outbox.Common;
using Accessory.Builder.Outbox.EntityFramework.Common;
using Accessory.Builder.Outbox.Schedulers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.Outbox.EntityFramework;

public static class Extensions
{
    public static IAccessoryBuilder AddEntityFrameworkOutbox<TDbContext>(this IAccessoryBuilder builder, string sectionName = "Outbox") where TDbContext : DbContext
    {
        if (!builder.TryRegisterAccessory(sectionName)) 
            return builder;
            
        builder.Services.AddScoped<IOutBoxRepository, OutBoxRepository<TDbContext>>();
        builder.Services.AddScoped<IDomainEventScheduler, DomainEventScheduler>();
        builder.Services.AddScoped<IOutboxEventDispatcher, OutboxEventDispatcher>();
            
        return builder;
    }
}