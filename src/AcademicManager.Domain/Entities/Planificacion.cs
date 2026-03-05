namespace AcademicManager.Domain.Entities;

/// <summary>
/// Planificación Anual - Documento estratégico que organiza el año académico
/// Estándares MINERD: Estructura completa de competencias, contenidos y evaluación
/// </summary>
public class Planificacion
{
    public int Id { get; set; }
    public int DocenteId { get; set; }
    public int CursoId { get; set; }
    public int PeriodoAcademicoId { get; set; }
    public int SeccionId { get; set; }

    // ═══════════════════════════════════════════════════════════
    // IDENTIFICACIÓN
    // ═══════════════════════════════════════════════════════════
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int AnoAcademico { get; set; } // Ej: 2024
    public string TipoplanificacionAcademico { get; set; } = "Anual"; // Anual, Mensual, Diaria

    // ═══════════════════════════════════════════════════════════
    // COMPETENCIAS MINERD
    // ═══════════════════════════════════════════════════════════
    public string CompetenciasFundamentales { get; set; } = string.Empty; // Saber ser, pensar, saber hacer
    public string CompetenciasEspecificas { get; set; } = string.Empty;   // Propias del área de conocimiento

    // ═══════════════════════════════════════════════════════════
    // CONTENIDOS MINERD (Triangulación de aprendizajes)
    // ═══════════════════════════════════════════════════════════
    public string ContenidosConceptuales { get; set; } = string.Empty;   // Hechos, conceptos, principios, leyes
    public string ContenidosProcedimentales { get; set; } = string.Empty; // Habilidades, técnicas, procedimientos
    public string ContenidosActitudinales { get; set; } = string.Empty;   // Valores, actitudes, normas

    // ═══════════════════════════════════════════════════════════
    // INDICADORES DE LOGRO
    // ═══════════════════════════════════════════════════════════
    public string IndicadoresLogro { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // ESTRATEGIAS Y RECURSOS
    // ═══════════════════════════════════════════════════════════
    public string EstrategiasEnsenanza { get; set; } = string.Empty;
    public string RecursosDidacticos { get; set; } = string.Empty;
    public string Recursos { get; set; } = string.Empty;
    public string EjesTransversales { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // EVALUACIÓN
    // ═══════════════════════════════════════════════════════════
    public string ActividadesEvaluacion { get; set; } = string.Empty;
    public string CriteriosEvaluacion { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // ADMINISTRATIVO
    // ═══════════════════════════════════════════════════════════
    public string Objetivos { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public string Metodologia { get; set; } = string.Empty;
    public string Evaluacion { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public DateTime FechaClase { get; set; }

    // ═══════════════════════════════════════════════════════════
    // CONTROL Y AUDITORÍA
    // ═══════════════════════════════════════════════════════════
    public string Estado { get; set; } = "Borrador"; // Borrador, Enviado, Aprobado, Ejecutándose, Evaluado
    public int? UsuarioAprobadorId { get; set; } // Director o Supervisor que aprobó
    public DateTime? FechaAprobacion { get; set; }
    public string? MotivoRechazo { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    // ═══════════════════════════════════════════════════════════
    // COMPATIBILIDAD HEREDADA
    // ═══════════════════════════════════════════════════════════
    public string Mes { get; set; } = string.Empty;
    public string TituloUnidad { get; set; } = string.Empty;
    public string SituacionAprendizaje { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // NAVEGACIÓN
    // ═══════════════════════════════════════════════════════════
    public Docente? Docente { get; set; }
    public Curso? Curso { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }
    public Seccion? Seccion { get; set; }
    public ICollection<PlanificacionMensual> PlanesMensuales { get; set; } = new List<PlanificacionMensual>();
    public ICollection<PlanificacionAuditoria> AuditoriaRegistros { get; set; } = new List<PlanificacionAuditoria>();
}
