using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionNotas.Infrastructure.Persistence.Configurations;

internal sealed class ProfesorConfiguration : IEntityTypeConfiguration<Profesor>
{
    public void Configure(EntityTypeBuilder<Profesor> builder)
    {
        builder.ToTable("Profesor");

        builder.HasKey(p => p.Id).HasName("PK_Profesor");

        builder.Property(p => p.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        // Único: no se permiten dos registros con el mismo nombre. La BD es la garantía final
        // (el servicio valida antes para dar un mensaje claro, pero dos peticiones simultáneas podrían pasar ambas).
        builder.HasIndex(p => p.Nombre).IsUnique().HasDatabaseName("IX_Profesor_Nombre");
    }
}
