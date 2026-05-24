using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SpecKitApi.Tests.Integration;

public sealed class ScalarUiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ScalarUiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetScalar_InProduction_ReturnsNotFound()
    {
        using var client = _factory
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Production");
            })
            .CreateClient();

        var response = await client.GetAsync("/scalar");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
