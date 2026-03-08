using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio para generar reportería consolidada para coordinadores.
/// Proporciona estadísticas de aprobaciones, regresión de docentes y alertas.
/// </summary>
public class ReportingService
{
    private readonly IPlanificacionRepository _planificacionRepository;
    private readonly IDocenteRepository _docenteRepository;
    private readonly ICursoRepository _cursoRepository;
    private readonly IGradeAuditTrailRepository _auditRepository;

    public ReportingService(
        IPlanificacionRepository planificacionRepository,
        IDocenteRepository docenteRepository,
        ICursoRepository cursoRepository,
        IGradeAuditTrailRepository auditRepository)
    {
        _planificacionRepository = planificacionRepository;
        _docenteRepository = docenteRepository;
        _cursoRepository = cursoRepository;
        _auditRepository = auditRepository;
    }

    /// <summary>
    /// Obtiene estadísticas de planificaciones por período.
    /// </summary>
    public async Task<ReporteAprobacionesDto> GetPlanificacionStatsAsync(int periodoId)
    {
        var planificaciones = (await _planificacionRepository.GetByPeriodoIdAsync(periodoId))
            .ToList();

        var stats = new ReporteAprobacionesDto
        {
            TotalPlanificaciones = planificaciones.Count,
            Borradores = planificaciones.Count(p => p.Estado == "Borrador"),
            Enviadas = planificaciones.Count(p => p.Estado == "Enviado"),
            Aprobadas = planificaciones.Count(p => p.Estado == "Aprobado"),
            Rechazadas = planificaciones.Count(p => p.Estado == "Rechazado")
        };

        stats.TasaAprobacionGlobal = stats.TotalPlanificaciones > 0
            ? (stats.Aprobadas / (decimal)stats.TotalPlanificaciones) * 100m
            : 0m;

        // Agrupar por estado
        var estadosUnicos = planificaciones.Select(p => p.Estado).Distinct();
        foreach (var estado in estadosUnicos)
        {
            var cantidad = planificaciones.Count(p => p.Estado == estado);
            var porcentaje = (cantidad / (decimal)stats.TotalPlanificaciones) * 100m;
            var docentes = planificaciones
                .Where(p => p.Estado == estado)
                .Select(p => p.DocenteId.ToString())
                .Distinct()
                .ToList();

            stats.EstadisticasPorEstado.Add(new PlanificacionStatsDto
            {
                Estado = estado,
                Cantidad = cantidad,
                Porcentaje = porcentaje,
                Docentes = docentes
            });
        }

        // Generar reporte de regresión de docentes
        stats.RegresionDocentes = await GenerateDocenteRegressionReportAsync(periodoId, planificaciones);

        // Generar alertas de vencidas
        stats.Alertas = GenerateExpiredPlanificacionAlerts(planificaciones);

        return stats;
    }

    /// <summary>
    /// Analiza el desempeño de docentes por período.
    /// </summary>
    public async Task<List<DocenteRegressionReportDto>> GenerateDocenteRegressionReportAsync(int periodoId, List<Planificacion> planificaciones)
    {
        var docentes = await _docenteRepository.GetAllAsync();
        var reporte = new List<DocenteRegressionReportDto>();

        foreach (var docente in docentes)
        {
            var planesDocente = planificaciones.Where(p => p.DocenteId == docente.Id).ToList();

            if (planesDocente.Count == 0)
                continue;

            var dto = new DocenteRegressionReportDto
            {
                DocenteId = docente.Id,
                NombreDocente = $"{docente.Nombres} {docente.Apellidos}",
                Email = docente.Email,
                PlanesEnviados = planesDocente.Count,
                PlanesAprobados = planesDocente.Count(p => p.Estado == "Aprobado"),
                PlanesRechazados = planesDocente.Count(p => p.Estado == "Rechazado"),
                TiempoPromedioAprobacionDias = 0m // Será calculado desde auditoría
            };

            // Determinar estado del docente
            var tasaAprobacion = dto.PlanesEnviados > 0
                ? (dto.PlanesAprobados / (decimal)dto.PlanesEnviados) * 100m
                : 0m;

            if (tasaAprobacion >= 90m)
                dto.Estado = "En regla";
            else if (tasaAprobacion >= 70m)
                dto.Estado = "Rezagado";
            else
                dto.Estado = "Crítico";

            reporte.Add(dto);
        }

        return reporte.OrderBy(d => d.Estado).ThenByDescending(d => d.PlanesAprobados).ToList();
    }

    /// <summary>
    /// Genera alertas de planificaciones próximas a vencer.
    /// </summary>
    public List<AlertaVencidaDto> GenerateExpiredPlanificacionAlerts(List<Planificacion> planificaciones)
    {
        var alertas = new List<AlertaVencidaDto>();

        // Considerar vencidas aquellas en estado "Enviado" por más de 14 días
        var planesEnviados = planificaciones
            .Where(p => p.Estado == "Enviado" && (DateTime.UtcNow - p.FechaModificacion).TotalDays > 14)
            .ToList();

        foreach (var plan in planesEnviados)
        {
            var diasVencido = (int)((DateTime.UtcNow - plan.FechaModificacion).TotalDays);

            alertas.Add(new AlertaVencidaDto
            {
                PlanificacionId = plan.Id,
                DocenteId = plan.DocenteId,
                NombreDocente = "Docente", // Se completa con datos de repositorio si es necesario
                Estado = plan.Estado,
                FechaVencimiento = plan.FechaModificacion.AddDays(14),
                DiasVencido = diasVencido
            });
        }

        return alertas.OrderByDescending(a => a.DiasVencido).ToList();
    }

    /// <summary>
    /// Exporta datos para reporte a Excel/PDF.
    /// </summary>
    public async Task<ReporteAprobacionesDto> ExportPlanificacionesStateReportAsync(
        int periodoId,
        int? gradoId = null,
        string? estado = null)
    {
        var planificaciones = (await _planificacionRepository.GetByPeriodoIdAsync(periodoId))
            .ToList();

        // Aplicar filtros
        if (gradoId.HasValue)
        {
            var cursosGrado = (await _cursoRepository.GetByGradoIdAsync(gradoId.Value))
                .Select(c => c.Id)
                .ToList();
            planificaciones = planificaciones
                .Where(p => cursosGrado.Contains(p.CursoId))
                .ToList();
        }

        if (!string.IsNullOrEmpty(estado))
        {
            planificaciones = planificaciones
                .Where(p => p.Estado == estado)
                .ToList();
        }

        return await GetPlanificacionStatsAsync(periodoId);
    }

    /// <summary>
    /// Obtiene porcentaje de cobertura de planificaciones por curso.
    /// </summary>
    public async Task<List<CoberturaCursoDto>> GetCoberturaByCursoAsync(int periodoId)
    {
        var cursos = (await _cursoRepository.GetAllAsync()).ToList();
        var planificaciones = (await _planificacionRepository.GetByPeriodoIdAsync(periodoId))
            .Where(p => p.Estado == "Aprobado")
            .ToList();

        var cobertura = new List<CoberturaCursoDto>();

        foreach (var curso in cursos)
        {
            var docentesCurso = planificaciones
                .Where(p => p.CursoId == curso.Id)
                .Select(p => p.DocenteId)
                .Distinct()
                .Count();

            cobertura.Add(new CoberturaCursoDto
            {
                CursoId = curso.Id,
                NombreCurso = curso.Nombre,
                TotalDocentes = 1, // Por lo general, 1 docente por curso
                DocentesConPlanificacion = docentesCurso,
                PorcentajeCoberturaInt = docentesCurso > 0 ? 100m : 0m
            });
        }

        return cobertura;
    }
}
