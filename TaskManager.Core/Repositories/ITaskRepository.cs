using System.Threading.Tasks;
using Accessory.Builder.Persistence.Core.Common;
using TaskManager.Core.Domain.User;

namespace TaskManager.Core.Repositories;

public interface ITaskRepository : IRepository<Domain.Task.Task, TaskId>
{
    Task<Core.Domain.Task.Task?> FindByTaskName(string name);
}