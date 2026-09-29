namespace GestionNotas.Domain.Entities;

public class Estudiante
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public ICollection<Nota> Notas { get; set; } = new List<Nota>();
}
