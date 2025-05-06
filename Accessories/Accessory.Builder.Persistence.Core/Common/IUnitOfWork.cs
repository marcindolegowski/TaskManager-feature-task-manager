using System.Threading.Tasks;

namespace Accessory.Builder.Persistence.Core.Common;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
}