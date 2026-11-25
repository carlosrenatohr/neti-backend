using MarcasAutos.Application.Abstractions;
using MarcasAutos.Application.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace MarcasAutos.Api.Controllers;

[ApiController]
[Route("api/v1/marcas-autos")]
public sealed class MarcasAutosController(IMarcasAutosService marcasAutosService) : ControllerBase
{
    // -- Thin controller: HTTP concerns only, the query lives in the application service --
    [HttpGet]
    [EndpointSummary("List car brands")]
    [EndpointDescription("Returns all car brands stored in the database, ordered by name.")]
    [ProducesResponseType<IReadOnlyList<MarcaAutoDto>>(
        StatusCodes.Status200OK,
        "application/json")]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError,
        "application/problem+json")]
    public async Task<ActionResult<IReadOnlyList<MarcaAutoDto>>> GetAll(CancellationToken cancellationToken)
    {
        var marcas = await marcasAutosService.GetAllAsync(cancellationToken);

        return Ok(marcas);
    }
}
