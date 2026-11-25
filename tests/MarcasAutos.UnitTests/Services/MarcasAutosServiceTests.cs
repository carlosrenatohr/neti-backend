using MarcasAutos.Application.Entities;
using MarcasAutos.Application.Services;
using MarcasAutos.Infrastructure.Persistence;
using MarcasAutos.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.UnitTests.Services;

public sealed class MarcasAutosServiceTests
{
    // -- Isolated context per test: no shared state between cases --
    private static MarcasAutosDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<MarcasAutosDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task GetAllAsync_ReturnsAllBrandsAsDtos()
    {
        await using var context = CreateContext();
        context.MarcasAutos.AddRange(
            new MarcaAuto { Id = 1, Nombre = "Toyota" },
            new MarcaAuto { Id = 2, Nombre = "Ford" });
        await context.SaveChangesAsync();

        var service = new MarcasAutosService(new MarcasAutosRepository(context));

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Collection(
            result,
            marca => Assert.Equal(new(2, "Ford"), marca),
            marca => Assert.Equal(new(1, "Toyota"), marca));
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenNoBrandsExist()
    {
        await using var context = CreateContext();
        var service = new MarcasAutosService(new MarcasAutosRepository(context));

        var result = await service.GetAllAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_PropagatesCancellation()
    {
        await using var context = CreateContext();
        var service = new MarcasAutosService(new MarcasAutosRepository(context));

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetAllAsync(cts.Token));
    }
}
