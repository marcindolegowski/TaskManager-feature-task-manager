using System;
using Accessory.Builder.Core.Common;
using Accessory.Builder.MessageBus.IntegrationEvent;

namespace Accessory.Builder.MessageBus;

public static class Extensions
{
    public static string GetEventFor<T>()
        where T : IIntegrationEvent
    {
        return GetEventForType(typeof(T));
    }

    public static string GetEventType(this IIntegrationEvent integrationEvent)
    {
        return GetEventForType(integrationEvent.GetType());
    }

    private static string GetEventForType(Type type)
    {
        var eventTypeName = type.Name.Replace("IntegrationEvent", "");
        var eventName = eventTypeName.Underscore().ToLower();

        return $"{eventName}";
    }
}