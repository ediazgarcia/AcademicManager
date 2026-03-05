namespace AcademicManager.Domain.Entities;

public class Tarea
{
    public int Id { get; set; }
    public int PlanificacionId { get; set; }
    public int CursoId { get; set; }
    public int PeriodoAcademicoId { get; set; }
    public int DocenteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaEntrega { get; set; }
    public int PuntosMaximos { get; set; } = 100;
    public bool PermiteEntregaTardia { get; set; } = false;
    public int DiasTardiosPermitidos { get; set; } = 0;
    public string TipoArchivoPermitido { get; set; } = "pdf,doc,docx,xls,xlsx,ppt,pptx,jpg,jpeg,png,gif,zip";
    public long TamanoMaximoArchivo { get; set; } = 10485760; // 10MB por defecto
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation
    public Planificacion? Planificacion { get; set; }
    public Curso? Curso { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }
    public Docente? Docente { get; set; }
}
