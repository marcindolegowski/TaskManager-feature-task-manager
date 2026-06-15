using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using TaskManager.Application.Task.Agents;

namespace TaskManager.Infrastructure.Agents;

/// <summary>
/// Adapter that forwards an implementation request to the Claude Agent SDK sidecar
/// over HTTP. The sidecar acknowledges immediately and runs the agent in the
/// background, so this call is fire-and-forget from the API's perspective.
/// </summary>
public class HttpAgentSidecarClient : IAgentSidecarClient
{
    // The sidecar reads camelCase JSON keys (taskId, repositoryUrl, credential.oauthToken).
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;

    public HttpAgentSidecarClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task RequestImplementationAsync(AgentImplementationRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/run", request, JsonOptions);
        response.EnsureSuccessStatusCode();
    }
}
