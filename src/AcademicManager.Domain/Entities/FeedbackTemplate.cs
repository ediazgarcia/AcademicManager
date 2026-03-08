namespace AcademicManager.Domain.Entities;

public class FeedbackTemplate
{
    public int Id { get; set; }
    public int DocenteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string Materia { get; set; } = string.Empty;
    public int Orden { get; set; } = 0;
    public bool Activa { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Usuario? Docente { get; set; }
}
