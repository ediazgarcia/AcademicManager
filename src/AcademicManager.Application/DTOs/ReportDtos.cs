namespace AcademicManager.Application.DTOs;

// DTOs para Reportería de Coordinador

public class PlanificacionStatsDto
{
    public string Estado { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Porcentaje { get; set; }
    public List<string> Docentes { get; set; } = [];
}

public class DocenteRegressionReportDto
{
    public int DocenteId { get; set; }
    public string NombreDocente { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PlanesEnviados { get; set; }
    public int PlanesAprobados { get; set; }
    public int PlanesRechazados { get; set; }
    public decimal TiempoPromedioAprobacionDias { get; set; }
    public string Estado { get; set; } = string.Empty; // "En regla", "Rezagado", "Crítico"
}

public class AprobacionTimelineDto
{
    public DateTime Fecha { get; set; }
    public int PlanesEnviados { get; set; }
    public int PlanesAprobados { get; set; }
    public int PlanesRechazados { get; set; }
    public decimal TasaAprobacion { get; set; }
}

public class AlertaVencidaDto
{
    public int PlanificacionId { get; set; }
    public int DocenteId { get; set; }
    public string NombreDocente { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public int DiasVencido { get; set; }
    public string CursoNombre { get; set; } = string.Empty;
}

public class CoberturaCursoDto
{
    public int CursoId { get; set; }
    public string NombreCurso { get; set; } = string.Empty;
    public int GradoId { get; set; }
    public string NombreGrado { get; set; } = string.Empty;
    public int TotalDocentes { get; set; }
    public int DocentesConPlanificacion { get; set; }
    public decimal PorcentajeCoberturaInt { get; set; }
}

public class ReportFiltrosDto
{
    public int? PeriodoId { get; set; }
    public int? GradoId { get; set; }
    public string? Estado { get; set; }
    public int? DocenteId { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}

public class ReporteAprobacionesDto
{
    public int TotalPlanificaciones { get; set; }
    public int Borradores { get; set; }
    public int Enviadas { get; set; }
    public int Aprobadas { get; set; }
    public int Rechazadas { get; set; }
    public decimal TasaAprobacionGlobal { get; set; }
    public List<PlanificacionStatsDto> EstadisticasPorEstado { get; set; } = [];
    public List<DocenteRegressionReportDto> RegresionDocentes { get; set; } = [];
    public List<AprobacionTimelineDto> Timeline { get; set; } = [];
    public List<AlertaVencidaDto> Alertas { get; set; } = [];
}

public class ExportReporteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string TipoReporte { get; set; } = string.Empty; // "Excel", "PDF"
    public ReportFiltrosDto Filtros { get; set; } = new();
}
