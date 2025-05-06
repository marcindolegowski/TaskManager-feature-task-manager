using System;
using Accessory.Builder.Core.Builders;
using Accessory.Builder.CQRS.IntegrationEvents.Common;
using Accessory.Builder.CQRS.IntegrationEvents.Dispatchers;
using Accessory.Builder.MessageBus.IntegrationEvent;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.CQRS.IntegrationEvents;

public static class Extensions
{
    public static IAccessoryBuilder AddCQRSIntegrationEvents(this IAccessoryBuilder builder, string sectionName = "CQRS.IntegrationEvents")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
            
        builder.AddIntegrationEventHandlers();
        builder.AddInMemoryIntegrationEventDispatcher();
        return builder;
    }
        
    private static IAccessoryBuilder AddIntegrationEventHandlers(this IAccessoryBuilder builder)
    {
        builder.Services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IIntegrationEventHandler<>)))
                .AsImplementedInterfaces()
                .AsSelf()
                .WithTransientLifetime());
        return builder;
    }
        
    private static IAccessoryBuilder AddInMemoryIntegrationEventDispatcher(this IAccessoryBuilder builder)
    {
        builder.Services.AddSingleton<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
        return builder;
    }
}