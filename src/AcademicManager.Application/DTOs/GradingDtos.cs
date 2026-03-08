namespace AcademicManager.Application.DTOs;

// DTOs para Calificación Unificada (Tareas + Evaluaciones)

public class BulkGradeEntryDto
{
    public int EntregaTareaId { get; set; }
    public int AlumnoId { get; set; }
    public string NombreAlumno { get; set; } = string.Empty;
    public string? NombreArch ivo { get; set; }
    public decimal? NotaActual { get; set; }
    public DateTime FechaEntrega { get; set; }
    public bool EsTardia { get; set; }
    public string Estado { get; set; } = string.Empty; // "Pendiente", "Calificada", "Tardía"
}

public class BulkGradeSubmissionDto
{
    public int EntregaTareaId { get; set; }
    public decimal Nota { get; set; }
    public string Retroalimentacion { get; set; } = string.Empty;
}

public class BulkGradingViewDto
{
    public int TareaId { get; set; }
    public string TareaTitulo { get; set; } = string.Empty;
    public int CursoId { get; set; }
    public string CursoNombre { get; set; } = string.Empty;
    public int PuntosMaximos { get; set; }
    public DateTime FechaEntrega { get; set; }
    public List<BulkGradeEntryDto> Entregas { get; set; } = [];
    public int TotalEntregas { get; set; }
    public int EntregasCalificadas { get; set; }
    public int EntregasPendientes { get; set; }
}

public class GradeComponentDto
{
    public string Tipo { get; set; } = string.Empty; // "Tarea", "Evaluacion"
    public string Nombre { get; set; } = string.Empty;
    public decimal Nota { get; set; }
    public decimal Peso { get; set; }
    public decimal NotaPonderada { get; set; }
}

public class ConsolidatedGradeDto
{
    public int AlumnoId { get; set; }
    public string NombreAlumno { get; set; } = string.Empty;
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public int PeriodoId { get; set; }
    public decimal NotaTareas { get; set; }
    public decimal NotaEvaluaciones { get; set; }
    public decimal NotaFinal { get; set; }
    public string Literal { get; set; } = string.Empty; // "Excelente", "Muy Bien", etc.
    public List<GradeComponentDto> Componentes { get; set; } = [];
    public decimal PesoPorcentajeTareas { get; set; } = 50m;
    public decimal PesoPorcentajeEvaluaciones { get; set; } = 50m;
}

public class GradeStatisticsDto
{
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public int? EvaluacionId { get; set; }
    public string? EvaluacionNombre { get; set; }
    public decimal Media { get; set; }
    public decimal Mediana { get; set; }
    public decimal Minima { get; set; }
    public decimal Maxima { get; set; }
    public decimal DesviacionEstandar { get; set; }
    public int EstudiantesTotales { get; set; }
    public int AprobadosCount { get; set; }
    public int DesaprobadosCount { get; set; }
    public decimal PorcentajeAprobacion { get; set; }
}

public class GradeChangeHistoryDto
{
    public int Id { get; set; }
    public int EntregaTareaId { get; set; }
    public int DocenteId { get; set; }
    public string NombreDocente { get; set; } = string.Empty;
    public decimal? NotaAnterior { get; set; }
    public decimal NotaNueva { get; set; }
    public string Razon { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

public class FeedbackTemplateDto
{
    public int Id { get; set; }
    public int DocenteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string Materia { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activa { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFeedbackTemplateDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string Materia { get; set; } = string.Empty;
    public int Orden { get; set; } = 0;
}

public class UpdateFeedbackTemplateDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string Materia { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activa { get; set; }
}
