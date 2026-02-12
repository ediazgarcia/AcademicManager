namespace AcademicManager.Domain.Entities;

public class Planificacion
{
    public int Id { get; set; }
    public int DocenteId { get; set; }
    public int CursoId { get; set; }
    public int PeriodoAcademicoId { get; set; }
    public int SeccionId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Objetivos { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string Metodologia { get; set; } = string.Empty;
    public string RecursosDidacticos { get; set; } = string.Empty;
    public string Recursos { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public string Evaluacion { get; set; } = string.Empty;
    public DateTime FechaClase { get; set; }
    public string Estado { get; set; } = "Borrador";       // Borrador, Enviado, Aprobado
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation
    public Docente? Docente { get; set; }
    public Curso? Curso { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }
    public Seccion? Seccion { get; set; }
}
