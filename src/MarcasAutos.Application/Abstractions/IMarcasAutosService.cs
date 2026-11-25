using MarcasAutos.Application.Contracts;

namespace MarcasAutos.Application.Abstractions;

public interface IMarcasAutosService
{
    Task<IReadOnlyList<MarcaAutoDto>> GetAllAsync(CancellationToken cancellationToken);
}
