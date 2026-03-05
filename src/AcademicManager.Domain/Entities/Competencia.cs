namespace AcademicManager.Domain.Entities;

/// <summary>
/// Competencia MINERD - Define las capacidades que deben desarrollar los estudiantes
/// Tipos: Fundamental (saber ser, pensar, saber hacer), Específica (del área de conocimiento)
/// </summary>
public class Competencia
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public int PeriodoAcademicoId { get; set; }

    public string Codigo { get; set; } = string.Empty; // Ej: "CF-1", "CE-1"
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Especifica"; // Fundamental, Especifica
    
    // Clasificación MINERD de competencias fundamentales
    public string? TipoFundamental { get; set; } = null; // SaberSer, SaberPensar, SaberHacer (solo si Tipo = Fundamental)

    public bool Activa { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation
    public Curso? Curso { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }
    public ICollection<Contenido> Contenidos { get; set; } = new List<Contenido>();
    public ICollection<IndicadorLogro> IndicadoresLogro { get; set; } = new List<IndicadorLogro>();
}
