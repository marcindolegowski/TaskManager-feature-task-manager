using Accessory.Builder.Core.Domain;

namespace TaskManager.Core.Domain.Rules;

public class UnsupportedTaskStatusException : IBusinessRule
{
    public string? BrokenRuleMessage => "Unsupported task status";

    public string Code => "unsupported-task-status";

    public bool IsValid() => true;
}
