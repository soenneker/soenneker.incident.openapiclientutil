using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Incident.HttpClients.Abstract;

namespace Soenneker.Incident.OpenApiClientUtil.Tests;

internal sealed class TestHttpClient(HttpClient client) : IIncidentOpenApiHttpClient
{
    public ValueTask<HttpClient> Get(CancellationToken cancellationToken = default) => ValueTask.FromResult(client);
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

