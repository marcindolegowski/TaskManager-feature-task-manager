using System.Collections.Generic;
using System.Threading.Tasks;
using Accessory.Builder.Core.Domain;

namespace Accessory.Builder.Persistence.Core.Common;

public interface IRepository<T, TId> where T : class, IAggregateRoot<TId> where TId : TypedIdValueBase
{
    Task<bool> ExistsAsync(TId id);
    void Add(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    Task<T?> FindByIdAsync(TId id);
}