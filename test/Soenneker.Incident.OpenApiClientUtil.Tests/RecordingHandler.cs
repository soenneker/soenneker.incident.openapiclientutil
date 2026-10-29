using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Incident.OpenApiClientUtil.Tests;

internal sealed class RecordingHandler : HttpMessageHandler
{
    public List<(string Url, string? Authorization)> Requests { get; } = [];
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public string Response { get; set; } = """{"identity":{"name":"test key","roles":["viewer"]}}""";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add((request.RequestUri!.AbsoluteUri, request.Headers.Authorization?.ToString()));
        return Task.FromResult(new HttpResponseMessage(StatusCode)
        {
            Content = new StringContent(Response, System.Text.Encoding.UTF8, "application/json")
        });
    }
}
