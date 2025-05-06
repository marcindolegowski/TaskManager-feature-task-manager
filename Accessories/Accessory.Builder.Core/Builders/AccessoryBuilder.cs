using System;
using System.Collections.Generic;
using System.Linq;
using Accessory.Builder.Core.Initializer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.Core.Builders;

public class AccessoryBuilder : IAccessoryBuilder
{
    public IServiceCollection Services { get; }
    private readonly List<Action<IServiceProvider>> _buildActions;
    private readonly IConfiguration? _configuration;
    private readonly List<string> _registeredAccessories;

    public AccessoryBuilder(IServiceCollection services, IConfiguration? configuration = null)
    {
        Services = services;
        Services.AddSingleton<IStartupInitializer>(new StartupInitializer());
        _registeredAccessories = new List<string>();
        _buildActions = new List<Action<IServiceProvider>>();

        if (configuration == null)
        {
            using var serviceProvider = Services.BuildServiceProvider();
            _configuration = serviceProvider.GetService<IConfiguration>();
        }
        else
        {
            _configuration = configuration;
        }
    }
        
    public bool TryRegisterAccessory(string AccessoryName)
    {
        var isAlreadyRegistered = _registeredAccessories.Any(r => r == AccessoryName);
        if (isAlreadyRegistered)
            return false;
        _registeredAccessories.Add(AccessoryName);
        return true;
    }

    public IServiceProvider Build()
    {
        var serviceProvider = Services.BuildServiceProvider();
        _buildActions.ForEach(a => a(serviceProvider));
        return serviceProvider;
    }

    public void AddInitializer<TInitializer>() where TInitializer : IInitializer
    {
        AddBuildAction(sp =>
        {
            var initializer = sp.GetRequiredService<TInitializer>();
            var startupInitializer = sp.GetRequiredService<IStartupInitializer>();
            startupInitializer.AddInitializer(initializer);
        });
    }

    public TProperties GetSettings<TProperties>(string appSettingSectionName) where TProperties : new()
    {
        return _configuration.GetSettings<TProperties>(appSettingSectionName);
    }

    public void GetSettings<TProperties>(string appSettingSectionName, TProperties properties)
        where TProperties : new()
    {
        if (_configuration != null) _configuration.GetSection(appSettingSectionName).Bind(properties);
    }
        
    private void AddBuildAction(Action<IServiceProvider> execute)
    {
        _buildActions.Add(execute);
    }

    public T? GetValue<T>(string key)
    {
        return _configuration.GetValue<T>(key);
    }
}