using Accessory.Builder.Core.Builders;
using Accessory.Builder.Persistence.Core.Common;
using Accessory.Persistence.EntityFramework.UnitOfWork.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Accessory.Persistence.EntityFramework.UnitOfWork;

public static class Extensions
{
    public static IAccessoryBuilder AddUnitOfWork(this IAccessoryBuilder builder, string sectionName = "UnitOfWork")
    {
        if (!builder.TryRegisterAccessory(sectionName))
            return builder;
            
        builder.Services.AddScoped<IDomainEventsAccessor, DomainEventsAccessor>();
        builder.Services.AddScoped<IUnitOfWork, Common.UnitOfWork>();
        builder.Services.AddScoped<ITransactionalUnitOfWork, Common.UnitOfWork>();
        return builder;
    }
}