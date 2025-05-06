using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Core.Domain.User;
using TaskManager.Core.Repositories;

namespace TaskManager.Infrastructure.Persistence.Repositories;

public class TaskRepository : DatabaseRepository<Core.Domain.Task.Task, TaskId>, ITaskRepository
{
    public TaskRepository(DatabaseContext context) : base(context) { }

    public Task<Core.Domain.Task.Task?> FindByTaskName(string name)
    {
        return _context.Set<Core.Domain.Task.Task>()
            .Where(x => x.Name == name).SingleOrDefaultAsync();
    }
}