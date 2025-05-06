using System.Threading.Tasks;
using Accessory.Builder.Persistence.Core.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Accessory.Persistence.EntityFramework.UnitOfWork.Common;

public interface ITransactionalUnitOfWork : IUnitOfWork
{
    bool HasActiveTransaction { get; }
    Task<IDbContextTransaction?> BeginTransactionAsync();
    Task CommitTransactionAsync(IDbContextTransaction transaction);
    void RollbackTransaction();
}