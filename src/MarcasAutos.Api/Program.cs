using MarcasAutos.Application;
using MarcasAutos.Infrastructure;
using MarcasAutos.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();

// -- Exposed for WebApplicationFactory-based integration tests --
public partial class Program;
