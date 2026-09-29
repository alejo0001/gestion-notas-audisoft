using GestionNotas.Application.Abstractions;
using GestionNotas.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestionNotas.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<Estudiante> Estudiantes => Set<Estudiante>();

    public DbSet<Profesor> Profesores => Set<Profesor>();

    public DbSet<Nota> Notas => Set<Nota>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Cada entidad tiene su propia clase IEntityTypeConfiguration (principio de responsabilidad única).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        SeedData.Apply(modelBuilder);
    }
}
