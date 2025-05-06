using Accessory.Builder.Core.Domain;

namespace TaskManager.Core.Domain.Rules;

public class TaskAlreadyCompletedException : IBusinessRule
{
    public string? BrokenRuleMessage => "Task already completed";

    public string Code => "task-already-completed";

    public bool IsValid() => true;
}
