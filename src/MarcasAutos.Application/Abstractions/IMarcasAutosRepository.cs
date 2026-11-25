using MarcasAutos.Application.Entities;

namespace MarcasAutos.Application.Abstractions;

// -- Application-owned port; Infrastructure provides the EF Core implementation --
public interface IMarcasAutosRepository
{
    Task<IReadOnlyList<MarcaAuto>> GetAllAsync(CancellationToken cancellationToken);
}
