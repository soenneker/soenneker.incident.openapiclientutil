using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Soenneker.Extensions.ValueTask;
using Soenneker.Incident.HttpClients.Abstract;
using Soenneker.Incident.OpenApiClientUtil.Abstract;
using Soenneker.Incident.OpenApiClient;
using Soenneker.Utils.AsyncSingleton;

namespace Soenneker.Incident.OpenApiClientUtil;

public sealed class IncidentOpenApiClientUtil : IIncidentOpenApiClientUtil
{
    private readonly AsyncSingleton<IncidentOpenApiClient> _client;

    public IncidentOpenApiClientUtil(IIncidentOpenApiHttpClient httpClientUtil, IConfiguration configuration)
    {
        _client = new AsyncSingleton<IncidentOpenApiClient>(async token =>
        {
            HttpClient httpClient = await httpClientUtil.Get(token).NoSync();

            Uri baseAddress = httpClient.BaseAddress ?? throw new InvalidOperationException("The Incident HTTP client requires a base address.");
            var authenticationProvider = new IncidentAuthenticationProvider(configuration, baseAddress);
            var requestAdapter = new HttpClientRequestAdapter(authenticationProvider, httpClient: httpClient)
            {
                BaseUrl = baseAddress.AbsoluteUri.TrimEnd('/')
            };
            return new IncidentOpenApiClient(requestAdapter);
        });
    }

    public ValueTask<IncidentOpenApiClient> Get(CancellationToken cancellationToken = default)
    {
        return _client.Get(cancellationToken);
    }

    public void Dispose()
    {
        _client.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }
}

