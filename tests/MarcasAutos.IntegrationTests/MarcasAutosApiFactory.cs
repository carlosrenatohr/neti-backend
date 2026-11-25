using MarcasAutos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MarcasAutos.IntegrationTests;

// -- Replaces PostgreSQL with a relational SQLite in-memory database for HTTP-level tests --
public sealed class MarcasAutosApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public MarcasAutosApiFactory() => _connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<MarcasAutosDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<MarcasAutosDbContext>>();

            services.AddDbContext<MarcasAutosDbContext>(options => options.UseSqlite(_connection));
        });
    }

    // -- EnsureCreated applies the model and the HasData seed configured in Infrastructure --
    public void InitializeDatabase()
    {
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<MarcasAutosDbContext>().Database.EnsureCreated();
    }

    public void ClearBrands()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MarcasAutosDbContext>();
        dbContext.MarcasAutos.ExecuteDelete();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
