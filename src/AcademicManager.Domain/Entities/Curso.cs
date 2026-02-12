namespace AcademicManager.Domain.Entities;

public class Curso
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int HorasSemanales { get; set; }
    public int Creditos { get; set; }
    public int? GradoId { get; set; }
    public bool Activo { get; set; } = true;

    // Navigation
    public Grado? Grado { get; set; }
}
