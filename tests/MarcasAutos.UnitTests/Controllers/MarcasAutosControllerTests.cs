using MarcasAutos.Api.Controllers;
using MarcasAutos.Application.Abstractions;
using MarcasAutos.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace MarcasAutos.UnitTests.Controllers;

public sealed class MarcasAutosControllerTests
{
    private readonly IMarcasAutosService _service = Substitute.For<IMarcasAutosService>();
    private readonly MarcasAutosController _controller;

    public MarcasAutosControllerTests() => _controller = new MarcasAutosController(_service);

    [Fact]
    public async Task GetAll_ReturnsOk_WithServicePayload()
    {
        IReadOnlyList<MarcaAutoDto> brands =
        [
            new(1, "Toyota"),
            new(2, "Ford")
        ];
        _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns(brands);

        var actionResult = await _controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Same(brands, ok.Value);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmptyList_WhenServiceReturnsNothing()
    {
        _service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MarcaAutoDto>());

        var actionResult = await _controller.GetAll(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Empty(Assert.IsAssignableFrom<IReadOnlyList<MarcaAutoDto>>(ok.Value));
    }

    [Fact]
    public async Task GetAll_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        _service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<MarcaAutoDto>());

        await _controller.GetAll(cts.Token);

        await _service.Received(1).GetAllAsync(cts.Token);
    }
}
