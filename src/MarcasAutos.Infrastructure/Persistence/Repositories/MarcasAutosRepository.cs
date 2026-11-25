using MarcasAutos.Application.Abstractions;
using MarcasAutos.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace MarcasAutos.Infrastructure.Persistence.Repositories;

public sealed class MarcasAutosRepository(MarcasAutosDbContext dbContext) : IMarcasAutosRepository
{
    public async Task<IReadOnlyList<MarcaAuto>> GetAllAsync(CancellationToken cancellationToken)
    {
        // -- Read-only query: no change tracking and deterministic ordering --
        return await dbContext.MarcasAutos
            .AsNoTracking()
            .OrderBy(marca => marca.Nombre)
            .ToListAsync(cancellationToken);
    }
}
