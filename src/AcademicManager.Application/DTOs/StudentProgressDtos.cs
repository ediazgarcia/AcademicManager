namespace AcademicManager.Application.DTOs;

// DTOs para Progreso del Estudiante

public enum RiskIndicatorEnum
{
    Verde,    // Excelente desempeño
    Amarillo, // Desempeño regular, necesita mejora
    Rojo      // Bajo desempeño, necesita intervención
}

public class EvaluacionProgressDto
{
    public int EvaluacionId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Nota { get; set; }
    public decimal Peso { get; set; }
    public DateTime FechaEvaluacion { get; set; }
}

public class CourseProgressDto
{
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public int DocenteId { get; set; }
    public string NombreDocente { get; set; } = string.Empty;
    public decimal NotaActual { get; set; }
    public decimal PromedioClase { get; set; }
    public int TareasEntregadas { get; set; }
    public int TareasPendientes { get; set; }
    public int TareasVencidas { get; set; }
    public RiskIndicatorEnum RiesgoAcademico { get; set; }
    public List<EvaluacionProgressDto> Evaluaciones { get; set; } = [];
    public string Literal { get; set; } = string.Empty; // "Excelente", "Bien", etc.
}

public class StudentProgressDashboardDto
{
    public int AlumnoId { get; set; }
    public string NombreAlumno { get; set; } = string.Empty;
    public string GradoNombre { get; set; } = string.Empty;
    public string SeccionNombre { get; set; } = string.Empty;
    public int PeriodoId { get; set; }
    public string NombrePeriodo { get; set; } = string.Empty;
    public List<CourseProgressDto> CursosPorCurso { get; set; } = [];
    public decimal PromedioPeriodo { get; set; }
    public RiskIndicatorEnum RiesgoAcademico { get; set; }
    public int CursosAprobados { get; set; }
    public int CursosDesaprobados { get; set; }
    public List<NotificacionDto> NotificacionesPendientes { get; set; } = [];
    public List<TareaProximaDto> TareasProximas { get; set; } = [];
}

public class TareaProximaDto
{
    public int TareaId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string NombreCurso { get; set; } = string.Empty;
    public DateTime FechaEntrega { get; set; }
    public int DiasRestantes { get; set; }
    public bool Vencida { get; set; }
    public int? PuntosActuales { get; set; }
    public int PuntosMaximos { get; set; }
    public bool EstaEntregada { get; set; }
    public string Estado { get; set; } = string.Empty; // "Pendiente", "Entregada", "Calificada", "Vencida"
}

public class PerformanceTrendDto
{
    public int PeriodoId { get; set; }
    public int NumeroPeriodo { get; set; }
    public string NombrePeriodo { get; set; } = string.Empty;
    public decimal Nota { get; set; }
    public decimal Cambio { get; set; } // vs período anterior (positivo = mejora)
    public string Tendencia { get; set; } = string.Empty; // "Mejorando", "Estable", "Decayendo"
}

public class NotificacionDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty; // "TaskDue", "GradePublished", "PlanificacionRejected"
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public bool Leida { get; set; }
    public int? RelatedEntityId { get; set; } // ID de la tarea, evaluación, etc.
}

public class StudentPerformanceTrendDto
{
    public int AlumnoId { get; set; }
    public string NombreAlumno { get; set; } = string.Empty;
    public List<PerformanceTrendDto> Tendencias { get; set; } = [];
    public decimal TendenciaGlobal { get; set; } // Promedio de cambios
}

public class CourseBudgetDto
{
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public decimal NotaTarea { get; set; }
    public decimal NotaEvaluaciones { get; set; }
    public decimal NotaFinal { get; set; }
    public List<GradeComponentDto> Componentes { get; set; } = [];
}

public class RiskIndicatorReportDto
{
    public int AlumnoId { get; set; }
    public string NombreAlumno { get; set; } = string.Empty;
    public RiskIndicatorEnum IndicadorRiesgo { get; set; }
    public int CursosEnRiesgo { get; set; }
    public List<string> CursosConBajaNota { get; set; } = [];
    public int TareasVencidas { get; set; }
    public string Recomendacion { get; set; } = string.Empty;
}

public class StudentRecommendationDto
{
    public int AlumnoId { get; set; }
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public decimal NotaActual { get; set; }
    public decimal NotaPromedioPorcentaje { get; set; }
    public int TareasPendientes { get; set; }
    public string Recomendacion { get; set; } = string.Empty;
    public bool NecesitaAyuda { get; set; }
}

public class StudentPeerComparisonDto
{
    public int AlumnoId { get; set; }
    public int CursoId { get; set; }
    public decimal NotaEstudiante { get; set; }
    public decimal PromedioClase { get; set; }
    public decimal Diferencia { get; set; }
    public int PosicionEnClase { get; set; }
    public int TotalEstudiantes { get; set; }
    public string CuartilPertenencia { get; set; } = string.Empty; // "Q1", "Q2", "Q3", "Q4"
}
