using Accessory.Builder.Core.Builders;
using Accessory.Builder.Core.Initializer;
using Accessory.Builder.MessageBus.Common;
using Accessory.Builder.MessageBus.IntegrationEvent;
using Accessory.Builder.MessageBus.ServiceBus.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using RabbitMQ.Client;

namespace Accessory.Builder.MessageBus.ServiceBus;

public static class Extensions
{
    public static IAccessoryBuilder AddServiceBus(this IAccessoryBuilder builder, string sectionName = "MessageBus")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
        var busProperties = builder.GetSettings<BusProperties>(sectionName);
        builder.Services.AddSingleton(busProperties);
        builder.Services.AddSingleton<IEventManager, EventManager>();
            
        if (busProperties.Host is null ||
            busProperties.User is null ||
            busProperties.Password is null ||
            busProperties.VirtualHost is null)
        {
            throw new ArgumentException($"{nameof(BusProperties)} could not be loaded from configuration. Please check, if section names are matching");
        }

        var appName = builder.GetValue<string>("App:Name");
        builder.Services.AddTransient<ChannelFactory>();
        builder.Services.AddSingleton<IConnectionProvider, ConnectionProvider>(sp =>
        {
            var factory = new ConnectionFactory
            {
                HostName = busProperties.Host,
                Port = busProperties.Port,
                UserName = busProperties.User,
                Password = busProperties.Password,
                VirtualHost = busProperties.VirtualHost,
            };

            var consumerConnection = factory.CreateConnection($"{appName}-consumer");
            var producerConnection = factory.CreateConnection($"{appName}-producer");

            var connectionProvider = new ConnectionProvider(consumerConnection, producerConnection);
            return connectionProvider;
        });

        return builder;
    }
        
    public static IAccessoryBuilder AddServiceBusPublisher<T>(this IAccessoryBuilder builder, string sectionName = "ServiceBusPublisher") where T : IIntegrationEvent
    {
        var eventType = MessageBus.Extensions.GetEventFor<T>();
        if (!builder.TryRegisterAccessory($"{eventType}_publisher"))
            return builder;
        var busProperties = builder.GetSettings<BusProperties>(sectionName);
        builder.Services.AddSingleton(busProperties);
        builder.Services.AddSingleton<IBusPublisher<T>, ServiceBusPublisher<T>>();
        return builder;
    }
        
    public static IAccessoryBuilder AddServiceBusSubscriber<TInit>(this IAccessoryBuilder builder, string sectionName = "ServiceBusSubscriber") where TInit : class, IInitializer
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
            
        builder.Services.AddSingleton<IServiceBusSubscriptionBuilder, ServiceBusSubscriptionBuilder>();
        builder.Services.AddTransient<TInit>();
        builder.AddInitializer<TInit>();
        builder.Services.AddSingleton<IEventSubscriber, ServiceBusEventSubscriber>();
            
        return builder;
    }
        
    public static IAccessoryBuilder AddServiceBusWorker(this IAccessoryBuilder builder, string sectionName = "ServiceBusWorker")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
        builder.Services.AddHostedService<WorkerServiceBus>();
        return builder;
    }
}