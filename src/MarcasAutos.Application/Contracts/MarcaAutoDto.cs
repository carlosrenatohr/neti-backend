namespace MarcasAutos.Application.Contracts;

// -- API contract kept separate from the persistence entity --
public sealed record MarcaAutoDto(int Id, string Nombre);
