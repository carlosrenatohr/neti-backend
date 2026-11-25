using MarcasAutos.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Infrastructure.Persistence;

public sealed class MarcasAutosDbContext(DbContextOptions<MarcasAutosDbContext> options) : DbContext(options)
{
    public DbSet<MarcaAuto> MarcasAutos => Set<MarcaAuto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // -- Keep entity configuration in dedicated classes, not in the DbContext --
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarcasAutosDbContext).Assembly);
    }
}
