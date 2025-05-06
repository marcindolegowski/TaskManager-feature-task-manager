using TaskManager.Core.Domain.Task;

namespace TaskManager.Application.Task.DTO;

public class TaskDTO
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public Status? Status { get; set; }
}