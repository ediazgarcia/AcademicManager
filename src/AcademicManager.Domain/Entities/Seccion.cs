namespace AcademicManager.Domain.Entities;

public class Seccion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;     // "A", "B", "C"
    public int GradoId { get; set; }
    public int Capacidad { get; set; }
    public bool Activo { get; set; } = true;

    // Navigation
    public Grado? Grado { get; set; }
}
