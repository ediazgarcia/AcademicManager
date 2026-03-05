namespace AcademicManager.Domain.Entities;

/// <summary>
/// Planificación Mensual - Deriva de una Planificación Anual
/// Organiza las actividades pedagógicas de un mes específico. Estándares MINERD.
/// </summary>
public class PlanificacionMensual
{
    public int Id { get; set; }
    
    /// <summary>FK a Planificacion (Anual) - Plan padre del que deriva</summary>
    public int PlanificacionId { get; set; }
    
    public int DocenteId { get; set; }
    public int CursoId { get; set; }
    public int SeccionId { get; set; }
    public int PeriodoAcademicoId { get; set; }

    // ═══════════════════════════════════════════════════════════
    // IDENTIFICACIÓN DEL MES
    // ═══════════════════════════════════════════════════════════
    public string Mes { get; set; } = string.Empty; // "Septiembre", "Octubre", etc.
    public int NumeroMes { get; set; } = 1;          // 1-12
    public string TituloUnidad { get; set; } = string.Empty;
    public string SituacionAprendizaje { get; set; } = string.Empty;
    
    // ═══════════════════════════════════════════════════════════
    // COMPETENCIAS MINERD
    // ═══════════════════════════════════════════════════════════
    public string CompetenciasFundamentales { get; set; } = string.Empty;
    public string CompetenciasEspecificas { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // CONTENIDOS (Triangulación de aprendizajes)
    // ═══════════════════════════════════════════════════════════
    public string ContenidosConceptuales { get; set; } = string.Empty;
    public string ContenidosProcedimentales { get; set; } = string.Empty;
    public string ContenidosActitudinales { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // ESTRATEGIAS, RECURSOS Y EVALUACIÓN
    // ═══════════════════════════════════════════════════════════
    public string IndicadoresLogro { get; set; } = string.Empty;
    public string EstrategiasEnsenanza { get; set; } = string.Empty;
    public string RecursosDidacticos { get; set; } = string.Empty;
    public string ActividadesEvaluacion { get; set; } = string.Empty;
    public string EjesTransversales { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;

    // ═══════════════════════════════════════════════════════════
    // CONTROL
    // ═══════════════════════════════════════════════════════════
    public string Estado { get; set; } = "Borrador"; // Borrador, Aprobado
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // ═══════════════════════════════════════════════════════════
    // NAVEGACIÓN
    // ═══════════════════════════════════════════════════════════
    public Planificacion? PlanificacionAnual { get; set; }
    public Docente? Docente { get; set; }
    public Curso? Curso { get; set; }
    public Seccion? Seccion { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }
    public ICollection<PlanificacionDiaria> PlanesDiarios { get; set; } = new List<PlanificacionDiaria>();
}
