using MarcasAutos.Application.Abstractions;
using MarcasAutos.Application.Contracts;

namespace MarcasAutos.Application.Services;

public sealed class MarcasAutosService(IMarcasAutosRepository repository) : IMarcasAutosService
{
    public async Task<IReadOnlyList<MarcaAutoDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var marcas = await repository.GetAllAsync(cancellationToken);

        // -- Map to DTOs so the API never depends on persistence entities --
        return marcas
            .Select(marca => new MarcaAutoDto(marca.Id, marca.Nombre))
            .ToArray();
    }
}
