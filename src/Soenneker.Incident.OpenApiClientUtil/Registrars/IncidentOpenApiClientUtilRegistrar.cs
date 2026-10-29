using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Incident.HttpClients.Registrars;
using Soenneker.Incident.OpenApiClientUtil.Abstract;

namespace Soenneker.Incident.OpenApiClientUtil.Registrars;

/// <summary>
/// Registers the OpenAPI client utility for dependency injection.
/// </summary>
public static class IncidentOpenApiClientUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IncidentOpenApiClientUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddIncidentOpenApiClientUtilAsSingleton(this IServiceCollection services)
    {
        services.AddIncidentOpenApiHttpClientAsSingleton()
                .TryAddSingleton<IIncidentOpenApiClientUtil, IncidentOpenApiClientUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IncidentOpenApiClientUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddIncidentOpenApiClientUtilAsScoped(this IServiceCollection services)
    {
        services.AddIncidentOpenApiHttpClientAsSingleton()
                .TryAddScoped<IIncidentOpenApiClientUtil, IncidentOpenApiClientUtil>();

        return services;
    }
}
