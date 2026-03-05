namespace AcademicManager.Domain.Entities;

/// <summary>
/// Indicador de Logro MINERD - Observable y medible que evidencia el desarrollo de competencias
/// Nivel: Alto (90-100), Medio (70-89), Bajo (50-69), No Iniciado (<50)
/// </summary>
public class IndicadorLogro
{
    public int Id { get; set; }
    public int CompetenciaId { get; set; }
    public int? ContenidoId { get; set; }
    public int PeriodoAcademicoId { get; set; }

    public string Codigo { get; set; } = string.Empty; // Ej: "IL-1.1"
    public string Descripcion { get; set; } = string.Empty; // Redacción en términos observables y medibles
    public string NivelExpectativa { get; set; } = "Medio"; // Alto, Medio, Bajo
    
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation
    public Competencia? Competencia { get; set; }
    public Contenido? Contenido { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }
}
