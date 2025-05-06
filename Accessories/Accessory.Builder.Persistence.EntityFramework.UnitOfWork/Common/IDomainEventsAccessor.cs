using System.Collections.Generic;
using Accessory.Builder.Core.Domain;

namespace Accessory.Persistence.EntityFramework.UnitOfWork.Common;

public interface IDomainEventsAccessor
{
    List<IDomainEvent> GetDomainEvents();
    void ClearAllDomainEvents();
}