using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application.Services;
using AcademicManager.Application.DTOs;

namespace AcademicManager.Web.Controllers;

/// <summary>
/// Controller para endpoints de reportería de coordinador.
/// Proporciona acceso a estadísticas, alertas y exportación de reportes.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "CoordinatorOrAdmin")]
public class ReportingController : ControllerBase
{
    private readonly ReportingService _reportingService;
    private readonly ILogger<ReportingController> _logger;

    public ReportingController(
        ReportingService reportingService,
        ILogger<ReportingController> logger)
    {
        _reportingService = reportingService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene estadísticas consolidadas de planificaciones por período.
    /// </summary>
    [HttpGet("planificaciones-stats")]
    public async Task<ActionResult<ReporteAprobacionesDto>> GetPlanificacionStats(int periodoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo estadísticas de planificaciones para período {periodoId}");
            var stats = await _reportingService.GetPlanificacionStatsAsync(periodoId);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, new { message = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Obtiene análisis de regresión de docentes (desempeño).
    /// </summary>
    [HttpGet("docente-regression")]
    public async Task<ActionResult<List<DocenteRegressionReportDto>>> GetDocenteRegression(int periodoId, int? docenteId = null)
    {
        try
        {
            _logger.LogInformation($"Obteniendo reporte de regresión para período {periodoId}");
            var planificaciones = new List<Planificacion>(); // Obtener desde servicio
            var reporte = await _reportingService.GenerateDocenteRegressionReportAsync(periodoId, planificaciones);

            if (docenteId.HasValue)
                reporte = reporte.Where(d => d.DocenteId == docenteId).ToList();

            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reporte de regresión");
            return StatusCode(500, new { message = "Error al obtener reporte" });
        }
    }

    /// <summary>
    /// Obtiene alertas de planificaciones vencidas sin aprobación.
    /// </summary>
    [HttpGet("alertas-vencidas")]
    public async Task<ActionResult<List<AlertaVencidaDto>>> GetAlertasVencidas(int periodoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo alertas vencidas para período {periodoId}");
            var stats = await _reportingService.GetPlanificacionStatsAsync(periodoId);
            return Ok(stats.Alertas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alertas");
            return StatusCode(500, new { message = "Error al obtener alertas" });
        }
    }

    /// <summary>
    /// Obtiene información de cobertura de planificaciones por curso.
    /// </summary>
    [HttpGet("cobertura-by-curso")]
    public async Task<ActionResult<List<CoberturaCursoDto>>> GetCoberturaByCurso(int periodoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo cobertura por curso para período {periodoId}");
            var cobertura = await _reportingService.GetCoberturaByCursoAsync(periodoId);
            return Ok(cobertura);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cobertura");
            return StatusCode(500, new { message = "Error al obtener cobertura" });
        }
    }

    /// <summary>
    /// Exporta reporte de planificaciones a Excel.
    /// </summary>
    [HttpPost("export-excel")]
    public async Task<ActionResult> ExportToExcel([FromBody] ReportFiltrosDto filtros)
    {
        try
        {
            _logger.LogInformation("Exportando reporte a Excel");

            if (filtros?.PeriodoId == null)
                return BadRequest(new { message = "Período es requerido" });

            var reporte = await _reportingService.ExportPlanificacionesStateReportAsync(
                filtros.PeriodoId.Value,
                filtros.GradoId,
                filtros.Estado);

            // Aquí se generaría archivo Excel
            // Por ahora retornamos JSON
            return Ok(new { message = "Exportación iniciada", data = reporte });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar reporte");
            return StatusCode(500, new { message = "Error al exportar reporte" });
        }
    }
}
