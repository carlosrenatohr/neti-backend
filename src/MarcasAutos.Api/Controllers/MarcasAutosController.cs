using MarcasAutos.Application.Abstractions;
using MarcasAutos.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MarcasAutos.Api.Controllers;

[ApiController]
[Route("api/v1/marcas-autos")]
[Produces("application/json")]
public sealed class MarcasAutosController(IMarcasAutosService marcasAutosService) : ControllerBase
{
    // -- Thin controller: HTTP concerns only, the query lives in the application service --
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MarcaAutoDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<MarcaAutoDto>>> GetAll(CancellationToken cancellationToken)
    {
        var marcas = await marcasAutosService.GetAllAsync(cancellationToken);

        return Ok(marcas);
    }
}
