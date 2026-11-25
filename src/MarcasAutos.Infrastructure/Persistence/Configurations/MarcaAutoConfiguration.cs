using MarcasAutos.Application.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MarcasAutos.Infrastructure.Persistence.Configurations;

public sealed class MarcaAutoConfiguration : IEntityTypeConfiguration<MarcaAuto>
{
    public void Configure(EntityTypeBuilder<MarcaAuto> builder)
    {
        builder.ToTable("MarcasAutos");

        builder.HasKey(marca => marca.Id);

        builder.Property(marca => marca.Id)
            .ValueGeneratedOnAdd();

        builder.Property(marca => marca.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(marca => marca.Nombre)
            .IsUnique();

        // -- Fixed identifiers keep seed data deterministic across migrations --
        builder.HasData(
            new MarcaAuto { Id = 1, Nombre = "Toyota" },
            new MarcaAuto { Id = 2, Nombre = "Ford" },
            new MarcaAuto { Id = 3, Nombre = "Honda" },
            new MarcaAuto { Id = 4, Nombre = "Chevrolet" });
    }
}
