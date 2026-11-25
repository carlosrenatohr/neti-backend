namespace MarcasAutos.Application.Entities;

// -- Persistence-agnostic model; EF Core mapping lives in Infrastructure --
public sealed class MarcaAuto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;
}
