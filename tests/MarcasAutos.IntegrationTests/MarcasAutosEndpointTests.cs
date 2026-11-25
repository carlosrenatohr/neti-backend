using System.Net;
using System.Net.Http.Json;
using MarcasAutos.Application.Contracts;

namespace MarcasAutos.IntegrationTests;

public sealed class MarcasAutosEndpointTests
{
    [Fact]
    public async Task GetMarcasAutos_ReturnsSeededBrands()
    {
        using var factory = new MarcasAutosApiFactory();
        factory.InitializeDatabase();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/marcas-autos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var brands = await response.Content.ReadFromJsonAsync<List<MarcaAutoDto>>();

        Assert.NotNull(brands);
        Assert.Equal(
            new[] { "Chevrolet", "Ford", "Honda", "Toyota" },
            brands.Select(brand => brand.Nombre));
    }

    [Fact]
    public async Task GetMarcasAutos_ReturnsEmptyArray_WhenDatabaseHasNoBrands()
    {
        using var factory = new MarcasAutosApiFactory();
        factory.InitializeDatabase();
        factory.ClearBrands();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/marcas-autos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var brands = await response.Content.ReadFromJsonAsync<List<MarcaAutoDto>>();

        Assert.NotNull(brands);
        Assert.Empty(brands);
    }

    [Fact]
    public async Task GetHealth_ReturnsHealthy()
    {
        using var factory = new MarcasAutosApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UnknownRoute_ReturnsProblemDetails()
    {
        using var factory = new MarcasAutosApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/unknown");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }
}
