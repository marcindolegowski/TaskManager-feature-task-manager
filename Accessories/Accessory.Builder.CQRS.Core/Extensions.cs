using System;
using Accessory.Builder.Core.Builders;
using Accessory.Builder.CQRS.Core.Commands;
using Accessory.Builder.CQRS.Core.Events;
using Accessory.Builder.CQRS.Core.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.CQRS.Core;

public static class Extensions
{
    public static IAccessoryBuilder AddCQRS(this IAccessoryBuilder builder, string sectionName = "CQRS")
    {
        if (!builder.TryRegisterAccessory(sectionName)) 
            return builder;

        builder.AddCommandHandlers();
        builder.AddInMemoryCommandDispatcher();

        builder.AddDomainEventHandlers();
        builder.AddInMemoryDomainEventDispatcher();

        builder.AddQueryHandlers();
        builder.AddInMemoryQueryDispatcher();

        return builder;
    }
        
    private static IAccessoryBuilder AddCommandHandlers(this IAccessoryBuilder builder, string sectionName = "Commands")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
            
        builder.Services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c =>
                {
                    c.AssignableTo(typeof(ICommandHandler<>));
                    c.Where((t) => !t.IsGenericType);
                })
                .AsImplementedInterfaces()
                .WithTransientLifetime());
        return builder;
    }
        
    private static IAccessoryBuilder AddInMemoryCommandDispatcher(this IAccessoryBuilder builder)
    {
        builder.Services.AddScoped<ICommandDispatcher, CommandDispatcher>();
        return builder;
    }

    private static IAccessoryBuilder AddDomainEventHandlers(this IAccessoryBuilder builder, string sectionName = "DomainEvents")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
            
        builder.Services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IDomainEventHandler<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
        return builder;
    }
    private static IAccessoryBuilder AddInMemoryDomainEventDispatcher(this IAccessoryBuilder builder)
    {
        builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        return builder;
    }

    private static IAccessoryBuilder AddQueryHandlers(this IAccessoryBuilder builder)
    {
        builder.Services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .AsSelf()
                .WithTransientLifetime());

        return builder;
    }

    private static IAccessoryBuilder AddInMemoryQueryDispatcher(this IAccessoryBuilder builder)
    {
        builder.Services.AddSingleton<IQueryDispatcher, QueryDispatcher>();
        return builder;
    }
}