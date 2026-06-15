using System.Threading.Tasks;

namespace TaskManager.Application.Agents;

/// <summary>Resolves which credential a run uses for a given developer.</summary>
public interface ICredentialResolver
{
    Task<ResolvedCredential?> ResolveAsync(string? userId);
}
