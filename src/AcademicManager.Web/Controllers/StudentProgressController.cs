using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application.Services;
using AcademicManager.Application.DTOs;

namespace AcademicManager.Web.Controllers;

/// <summary>
/// Controller para endpoints de progreso académico del estudiante.
/// Proporciona dashboards, tendencias y análisis de desempeño.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class StudentProgressController : ControllerBase
{
    private readonly StudentProgressService _progressService;
    private readonly ILogger<StudentProgressController> _logger;

    public StudentProgressController(
        StudentProgressService progressService,
        ILogger<StudentProgressController> logger)
    {
        _progressService = progressService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene dashboard completo de progreso de un estudiante.
    /// </summary>
    [HttpGet("{alumnoId}/dashboard")]
    public async Task<ActionResult<StudentProgressDashboardDto>> GetProgressDashboard(int alumnoId, int periodoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo dashboard de progreso para alumno {alumnoId}");
            var dashboard = await _progressService.GetProgressDashboardAsync(alumnoId, periodoId);
            return Ok(dashboard);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener dashboard");
            return StatusCode(500, new { message = "Error al obtener dashboard" });
        }
    }

    /// <summary>
    /// Obtiene tendencias de desempeño del estudiante.
    /// </summary>
    [HttpGet("{alumnoId}/trends")]
    public async Task<ActionResult<StudentPerformanceTrendDto>> GetPerformanceTrend(int alumnoId, int ultimosPeriodos = 4)
    {
        try
        {
            _logger.LogInformation($"Obteniendo tendencias para alumno {alumnoId}");
            var trends = await _progressService.GetPerformanceTrendAsync(alumnoId, ultimosPeriodos);
            return Ok(trends);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tendencias");
            return StatusCode(500, new { message = "Error al obtener tendencias" });
        }
    }

    /// <summary>
    /// Obtiene indicador de riesgo académico del estudiante.
    /// </summary>
    [HttpGet("{alumnoId}/risk-indicator")]
    public async Task<ActionResult<RiskIndicatorReportDto>> GetRiskIndicator(int alumnoId, int periodoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo indicador de riesgo para alumno {alumnoId}");
            var riskReport = await _progressService.GetRiskIndicatorAsync(alumnoId, periodoId);
            return Ok(riskReport);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener indicador de riesgo");
            return StatusCode(500, new { message = "Error al obtener indicador" });
        }
    }

    /// <summary>
    /// Compara desempeño del estudiante con promedio de clase.
    /// </summary>
    [HttpGet("{alumnoId}/peer-comparison")]
    public async Task<ActionResult<StudentPeerComparisonDto>> CompareToPeer(int alumnoId, int cursoId)
    {
        try
        {
            _logger.LogInformation($"Comparando alumno {alumnoId} con compañeros");
            var comparison = await _progressService.CompareToPeerAsync(alumnoId, cursoId);
            return Ok(comparison);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al comparar desempeño");
            return StatusCode(500, new { message = "Error al obtener comparación" });
        }
    }

    /// <summary>
    /// Obtiene recomendaciones personalizadas de mejora académica.
    /// </summary>
    [HttpGet("{alumnoId}/recommendations")]
    public async Task<ActionResult<List<StudentRecommendationDto>>> GetRecommendations(int alumnoId, int cursoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo recomendaciones para alumno {alumnoId}");
            var recommendations = await _progressService.GenerateRecommendationsAsync(alumnoId, cursoId);
            return Ok(recommendations);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener recomendaciones");
            return StatusCode(500, new { message = "Error al obtener recomendaciones" });
        }
    }

    /// <summary>
    /// Obtiene desglose de calificaciones por curso.
    /// </summary>
    [HttpGet("{alumnoId}/course-breakdown")]
    public async Task<ActionResult<CourseBudgetDto>> GetCourseGradeBreakdown(int alumnoId, int cursoId, int periodoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo desglose de calificaciones");
            var breakdown = await _progressService.GetCourseGradeBreakdownAsync(alumnoId, cursoId, periodoId);
            return Ok(breakdown);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener desglose");
            return StatusCode(500, new { message = "Error al obtener desglose" });
        }
    }
}
