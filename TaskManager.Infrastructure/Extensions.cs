using System;
using Accessory.Builder.Core.Builders;
using Accessory.Builder.Core.Domain.Exceptions;
using Accessory.Builder.CQRS.Core;
using Accessory.Builder.CQRS.Core.Commands;
using Accessory.Builder.CQRS.Dapper;
using Accessory.Builder.CQRS.IntegrationEvents;
using Accessory.Builder.Logging.OpenTelemetry.Decorators;
using Accessory.Builder.MessageBus.IntegrationEvent;
using Accessory.Builder.MessageBus.ServiceBus;
using Accessory.Builder.Outbox.EntityFramework;
using Accessory.Builder.Outbox.EntityFramework.Common;
using Accessory.Builder.Persistence.EntityFramework;
using Accessory.Builder.Redis;
using Accessory.Builder.RunningContext;
using Accessory.Builder.WebApi;
using Accessory.Builder.WebApi.Exceptions.Types;
using Accessory.Builder.WebApi.RunningContext;
using Accessory.Persistence.EntityFramework.Audit;
using Accessory.Persistence.EntityFramework.Audit.Common;
using Accessory.Persistence.EntityFramework.UnitOfWork;
using Accessory.Persistence.EntityFramework.UnitOfWork.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Task.Events;
using TaskManager.Application.User.Events;
using TaskManager.Infrastructure.Events;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Repositories;
using TaskManager.Infrastructure.ReadModel;

namespace TaskManager.Infrastructure;

public static class Extensions
{
    public static IAccessoryBuilder AddInfrastructure(this IAccessoryBuilder builder)
    {
        // Defining database context
        builder.AddEntityFramework<DatabaseContext>();
        builder.AddEntityFrameworkAuditTrail<DatabaseContext>();
        builder.AddEntityFrameworkOutbox<DatabaseContext>();
        builder.AddUnitOfWork();
        builder.AddRedisCacheIntegration();
        builder.Services.RegisterRepositories();
            
        // Defining CQRS
        builder.AddCQRS();
        builder.AddCQRSIntegrationEvents();
        builder.AddDapperForQueries(new DapperInitializer());
            
        // Defining decorators
        builder.Services.Decorate(typeof(ICommandHandler<>), typeof(UnitOfWorkCommandHandlerDecorator<>));
        builder.Services.Decorate(typeof(ICommandHandler<>), typeof(AuditTrailCommandHandlerDecorator<>));
        builder.Services.Decorate(typeof(ICommandHandler<>), typeof(OutboxHandlerDecorator<>));
        builder.Services.Decorate(typeof(ICommandHandler<>), typeof(OpenTelemetryLoggingCommandHandlerDecorator<>));
        // Defining the source of request and its context
        builder.AddRunningContext(x => x.GetService<IHttpRunningContextProvider>());
            
        // Defining the error handling strategy
        builder.AddErrorHandler(c =>
        {
            c.Map<BrokenBusinessRuleException>((http, ex) => new BrokenBusinessRuleProblemDetails(ex));
            c.Map<CommandNotValidException>((http, ex) => new CommandNotValidProblemDetails(ex));
            c.Map<QueryNotValidException>((http, ex) => new QueryNotValidProblemDetails(ex));
            c.Map<DbUpdateConcurrencyException>((http, ex) => new ConcurrencyProblemDetails());
        });

        // ServiceBus register events
        builder.AddServiceBus();
        builder.AddServiceBusSubscriber<ServiceBusSubscriptionRegistrationInitializer>();
        builder.AddServiceBusWorker();
        builder.AddServiceBusPublisher<RemovalTaskEvent>();
        builder.AddServiceBusPublisher<TaskCompletedEvent>();

        return builder;
    }

    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder builder)
    {
        // ServiceBus register event example
        var busSubscriber = builder.ApplicationServices.GetRequiredService<IEventSubscriber>();
        busSubscriber.Subscribe<RemovalTaskEvent, RemovalTaskEventHandler>();
        busSubscriber.Subscribe<TaskCompletedEvent, TaskCompletedEventHandler>();
        return builder;
    }
        
    private static void RegisterRepositories(this IServiceCollection services)
    {
        services.Scan(s =>
            s.FromAssemblies(AppDomain.CurrentDomain.GetAssemblies())
                .AddClasses(c => 
                    c.AssignableTo(typeof(DatabaseRepository<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());
    }
}