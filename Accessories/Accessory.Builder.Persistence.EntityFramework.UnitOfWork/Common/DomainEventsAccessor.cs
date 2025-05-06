using System.Collections.Generic;
using System.Linq;
using Accessory.Builder.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Accessory.Persistence.EntityFramework.UnitOfWork.Common;

public class DomainEventsAccessor : IDomainEventsAccessor
{
    private readonly DbContext _context;

    public DomainEventsAccessor(DbContext context)
    {
        _context = context;
    }

    public void ClearAllDomainEvents()
    {
        _context.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());
    }

    public List<IDomainEvent> GetDomainEvents()
    {
        return _context.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();
    }
}