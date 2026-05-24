using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpecKitApi.Clients;
using SpecKitApi.Options;
using SpecKitApi.Services;

namespace SpecKitApi.Extensions;

/// <summary>
/// Extension methods for registering JSONPlaceholder services in the DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the JSONPlaceholder typed HTTP client, resilience policies,
    /// and the album service in the DI container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection AddJsonPlaceholderServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JsonPlaceholderOptions>(
            configuration.GetSection(JsonPlaceholderOptions.SectionName));

        var options = configuration
            .GetSection(JsonPlaceholderOptions.SectionName)
            .Get<JsonPlaceholderOptions>() ?? new JsonPlaceholderOptions();

        services
            .AddHttpClient<IJsonPlaceholderClient, JsonPlaceholderClient>(client =>
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            })
            .AddStandardResilienceHandler();

        services.AddScoped<IAlbumService, AlbumService>();

        return services;
    }
}
