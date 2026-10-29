using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Soenneker.Incident.OpenApiClient.Models;

namespace Soenneker.Incident.OpenApiClientUtil.Tests;

public sealed class IncidentOpenApiClientUtilTests
{
    private static IConfiguration Configuration() => new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Incident:ApiKey"] = "first"
    }).Build();

    [Test]
    public async Task UsesConfiguredBaseUrlAndCurrentApiKey()
    {
        IConfiguration configuration = Configuration();
        using var handler = new RecordingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://proxy.example/incident/") };
        await using var utility = new IncidentOpenApiClientUtil(new TestHttpClient(http), configuration);
        var client = await utility.Get();
        if (!ReferenceEquals(client, await utility.Get())) throw new Exception("The client was not cached.");
        var identity = await client.V1.Identity.GetAsync();
        configuration["Incident:ApiKey"] = "rotated";
        await client.V1.Identity.GetAsync();
        if (identity?.Identity?.Name != "test key" || handler.Requests[0].Url != "https://proxy.example/incident/v1/identity" ||
            handler.Requests[0].Authorization != "Bearer first" || handler.Requests[1].Authorization != "Bearer rotated")
            throw new Exception("Identity response, URL, or authentication was incorrect.");
    }

    [Test]
    public async Task PreservesPaginationCursorAndPageSize()
    {
        using var handler = new RecordingHandler { Response = """{"incidents":[],"pagination_meta":{"after":"next-page","page_size":2,"total_record_count":4}}""" };
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.incident.io/") };
        await using var utility = new IncidentOpenApiClientUtil(new TestHttpClient(http), Configuration());
        var client = await utility.Get();
        var response = await client.V2.Incidents.GetAsync(request =>
        {
            request.QueryParameters.After = "previous-page";
            request.QueryParameters.PageSize = 2;
        });
        if (!handler.Requests[0].Url.Contains("after=previous-page") || !handler.Requests[0].Url.Contains("page_size=2") ||
            response?.PaginationMeta?.After != "next-page")
            throw new Exception("Pagination was not preserved.");
    }

    [Test]
    public async Task DeserializesApiErrors()
    {
        using var handler = new RecordingHandler
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Response = """{"type":"authentication_error","status":401,"request_id":"request-test","errors":[{"code":"missing_authorization_material","message":"Missing API key"}]}"""
        };
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.incident.io/") };
        await using var utility = new IncidentOpenApiClientUtil(new TestHttpClient(http), Configuration());
        var client = await utility.Get();
        try { await client.V1.Identity.GetAsync(); }
        catch (ErrorResponse error)
        {
            if (error.Status != 401 || error.RequestId != "request-test" || error.Errors?.Count != 1)
                throw new Exception("Structured API error details were lost.");
            return;
        }
        throw new Exception("An API error was accepted as success.");
    }

    [Test]
    public async Task RejectsUntrustedHostsAndMissingCredentials()
    {
        IConfiguration configuration = Configuration();
        using var handler = new RecordingHandler();
        using var http = new HttpClient(handler) { BaseAddress = new Uri("https://api.incident.io/") };
        await using var utility = new IncidentOpenApiClientUtil(new TestHttpClient(http), configuration);
        var client = await utility.Get();
        bool rejected = false;
        try { await client.V1.Identity.WithUrl("https://other.example/v1/identity").GetAsync(); }
        catch (InvalidOperationException) { rejected = true; }
        if (!rejected || handler.Requests.Count != 0) throw new Exception("An untrusted host was accepted.");
        configuration["Incident:ApiKey"] = "";
        try { await client.V1.Identity.GetAsync(); }
        catch (InvalidOperationException)
        {
            if (handler.Requests.Count != 0) throw new Exception("A request was sent without credentials.");
            return;
        }
        throw new Exception("Missing credentials were accepted.");
    }
}
