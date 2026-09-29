namespace GestionNotas.Domain.Entities;

public class Nota
{
    /// <summary>Nota mínima para aprobar en la escala colombiana de 0 a 5.</summary>
    public const decimal NotaMinimaAprobatoria = 3.0m;

    public const decimal ValorMinimo = 0m;

    public const decimal ValorMaximo = 5m;

    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int IdProfesor { get; set; }

    public int IdEstudiante { get; set; }

    /// <summary>
    /// decimal (no double/float): las calificaciones deben guardarse exactas, sin errores de redondeo binario.
    /// </summary>
    public decimal Valor { get; set; }

    public Profesor Profesor { get; set; } = null!;

    public Estudiante Estudiante { get; set; } = null!;
}
