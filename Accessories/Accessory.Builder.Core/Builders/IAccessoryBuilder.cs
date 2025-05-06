using System;
using Accessory.Builder.Core.Initializer;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.Core.Builders;

public interface IAccessoryBuilder
{
    IServiceCollection Services { get; }

    bool TryRegisterAccessory(string AccessoryName);
        
    IServiceProvider Build();
        
    void AddInitializer<TInitializer>() where TInitializer : IInitializer;
        
    TProperties GetSettings<TProperties>(string appSettingSectionName) where TProperties : new();

    void GetSettings<TProperties>(string appSettingSectionName, TProperties properties) where TProperties : new();

    T? GetValue<T>(string key);
}