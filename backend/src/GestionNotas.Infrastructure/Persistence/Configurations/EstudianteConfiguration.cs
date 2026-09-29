using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionNotas.Infrastructure.Persistence.Configurations;

internal sealed class EstudianteConfiguration : IEntityTypeConfiguration<Estudiante>
{
    public void Configure(EntityTypeBuilder<Estudiante> builder)
    {
        builder.ToTable("Estudiante");

        builder.HasKey(e => e.Id).HasName("PK_Estudiante");

        builder.Property(e => e.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        // Único: no se permiten dos registros con el mismo nombre. La BD es la garantía final
        // (el servicio valida antes para dar un mensaje claro, pero dos peticiones simultáneas podrían pasar ambas).
        builder.HasIndex(e => e.Nombre).IsUnique().HasDatabaseName("IX_Estudiante_Nombre");
    }
}
