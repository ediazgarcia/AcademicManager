namespace AcademicManager.Domain.Entities;

/// <summary>
/// Contenido MINERD - Componentes de la triangulación de aprendizajes
/// Tipos: Conceptual (saber qué), Procedimental (saber hacer), Actitudinal (saber ser)
/// </summary>
public class Contenido
{
    public int Id { get; set; }
    public int CompetenciaId { get; set; }
    public int PeriodoAcademicoId { get; set; }

    public string Codigo { get; set; } = string.Empty; // Ej: "CONT-1"
    public string Descripcion { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Conceptual"; // Conceptual, Procedimental, Actitudinal

    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation
    public Competencia? Competencia { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }
    public ICollection<IndicadorLogro> IndicadoresLogro { get; set; } = new List<IndicadorLogro>();
}
