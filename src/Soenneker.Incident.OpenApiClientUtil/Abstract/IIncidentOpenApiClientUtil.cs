using Soenneker.Incident.OpenApiClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Incident.OpenApiClientUtil.Abstract;

/// <summary>
/// Exposes a cached OpenAPI client instance.
/// </summary>
public interface IIncidentOpenApiClientUtil: IDisposable, IAsyncDisposable
{
    /// <summary>Gets the cached client. Reads Incident:ApiKey for every request and sends it as a bearer token. Configure Incident:ClientBaseUrl before first use; the default is https://api.incident.io.</summary>
    ValueTask<IncidentOpenApiClient> Get(CancellationToken cancellationToken = default);
}
