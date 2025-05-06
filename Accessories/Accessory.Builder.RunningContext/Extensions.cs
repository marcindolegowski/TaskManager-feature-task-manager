using Accessory.Builder.Core.Builders;
using Accessory.Builder.RunningContext.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Builder.RunningContext;

public static class Extensions
{
    public static IAccessoryBuilder AddRunningContext(this IAccessoryBuilder builder, Func<IServiceProvider, IRunningContextProvider?> contextFactory, string sectionName = "RunningContext")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;

        builder.Services.AddScoped(contextFactory);
        return builder;
    }
}