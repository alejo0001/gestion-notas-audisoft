using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GestionNotas.Infrastructure.Persistence.Configurations;

internal sealed class NotaConfiguration : IEntityTypeConfiguration<Nota>
{
    public void Configure(EntityTypeBuilder<Nota> builder)
    {
        builder.ToTable("Nota", table => table.HasCheckConstraint(
            "CK_Nota_Valor",
            "[Valor] >= 0 AND [Valor] <= 5"));

        builder.HasKey(n => n.Id).HasName("PK_Nota");

        builder.Property(n => n.Nombre)
            .HasMaxLength(100)
            .IsRequired();

        // DECIMAL(3,2): de 0.00 a 9.99, suficiente para la escala 0–5 con dos decimales.
        builder.Property(n => n.Valor).HasPrecision(3, 2);

        // Restricciones de llave foránea exigidas por el enunciado.
        // Restrict = ON DELETE NO ACTION: no se pierden notas al borrar un estudiante/profesor (ADR 0005).
        builder.HasOne(n => n.Estudiante)
            .WithMany(e => e.Notas)
            .HasForeignKey(n => n.IdEstudiante)
            .HasConstraintName("FK_Nota_Estudiante")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Profesor)
            .WithMany(p => p.Notas)
            .HasForeignKey(n => n.IdProfesor)
            .HasConstraintName("FK_Nota_Profesor")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.IdEstudiante).HasDatabaseName("IX_Nota_IdEstudiante");
        builder.HasIndex(n => n.IdProfesor).HasDatabaseName("IX_Nota_IdProfesor");
    }
}
