using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task GetOpenApiDocument_ExposesTheRealContract()
    {
        using var factory = new MarcasAutosApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;

        Assert.Equal("Marcas Autos API", root.GetProperty("info").GetProperty("title").GetString());
        Assert.Equal("v1", root.GetProperty("info").GetProperty("version").GetString());

        var operation = root.GetProperty("paths")
            .GetProperty("/api/v1/marcas-autos")
            .GetProperty("get");

        Assert.Contains("List car brands", operation.GetProperty("summary").GetString());
        Assert.True(operation.GetProperty("responses").TryGetProperty("200", out _));
    }

    [Fact]
    public async Task GetScalar_ReturnsHtmlThatLoadsTheOpenApiDocument()
    {
        using var factory = new MarcasAutosApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/scalar");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("openapi/v1.json", await response.Content.ReadAsStringAsync());
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
