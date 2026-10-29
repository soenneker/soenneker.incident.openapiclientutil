using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Soenneker.Incident.OpenApiClientUtil;

internal sealed class IncidentAuthenticationProvider(IConfiguration configuration, Uri baseAddress) : IAuthenticationProvider
{
    public Task AuthenticateRequestAsync(RequestInformation request, Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Uri uri = request.URI;
        if (uri.Scheme != Uri.UriSchemeHttps || !uri.Authority.Equals(baseAddress.Authority, StringComparison.OrdinalIgnoreCase) ||
            !uri.AbsolutePath.StartsWith(baseAddress.AbsolutePath.TrimEnd('/') + "/", StringComparison.Ordinal))
            throw new InvalidOperationException("Incident credentials may only be sent to the configured HTTPS API base URL.");

        string apiKey = Required("Incident:ApiKey");
        request.Headers.Remove("Authorization");
        request.Headers.Add("Authorization", "Bearer " + apiKey);

        return Task.CompletedTask;
    }

    private string Required(string key)
    {
        string? value = configuration[key];
        return !string.IsNullOrWhiteSpace(value) ? value
            : throw new InvalidOperationException(key + " is required.");
    }
}

