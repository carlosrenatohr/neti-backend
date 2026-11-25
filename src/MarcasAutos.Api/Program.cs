using MarcasAutos.Application;
using MarcasAutos.Infrastructure;
using MarcasAutos.Infrastructure.Persistence;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// -- Render (and similar hosts) inject PORT at runtime; bind to it when present --
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();

// -- OpenAPI document generated from the real controllers and DTOs --
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info.Title = "Marcas Autos API";
        document.Info.Version = "v1";
        document.Info.Description =
            "Read-only API that exposes the car brands stored in PostgreSQL.";

        // -- Host-agnostic document: clients and Scalar resolve the current origin --
        document.Servers.Clear();
        return Task.CompletedTask;
    });
});

// -- Standardized RFC 7807 responses for unhandled exceptions and error status codes --
builder.Services.AddProblemDetails();

// -- Composition root: layers expose their own registration extensions --
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<MarcasAutosDbContext>("database");

var app = builder.Build();

// -- Centralized exception handling avoids leaking internals to clients --
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApi();

// -- Scalar is the only visual documentation UI, fed by the generated document --
app.MapScalarApiReference(options => options
    .WithTitle("Marcas Autos API")
    .WithOpenApiRoutePattern("/openapi/{documentName}.json"));

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

// -- Exposed for WebApplicationFactory-based integration tests --
public partial class Program;
