using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application.Services;
using AcademicManager.Application.DTOs;

namespace AcademicManager.Web.Controllers;

/// <summary>
/// Controller para endpoints de calificación unificada.
/// Proporciona acceso a vista bulk de calificación y estadísticas de evaluaciones.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "DocenteOrAdmin")]
public class GradingController : ControllerBase
{
    private readonly GradingUnifyService _gradingUnifyService;
    private readonly ILogger<GradingController> _logger;

    public GradingController(
        GradingUnifyService gradingUnifyService,
        ILogger<GradingController> logger)
    {
        _gradingUnifyService = gradingUnifyService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene vista de calificación en lote para una tarea.
    /// </summary>
    [HttpGet("task/{tareaId}/bulk-view")]
    public async Task<ActionResult<BulkGradingViewDto>> GetBulkGradingView(int tareaId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo vista bulk para tarea {tareaId}");
            var view = await _gradingUnifyService.GetBulkGradingViewAsync(tareaId);
            return Ok(view);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener vista bulk");
            return StatusCode(500, new { message = "Error al obtener datos" });
        }
    }

    /// <summary>
    /// Califica múltiples entregas de una tarea (bulk grading).
    /// </summary>
    [HttpPost("bulk-grade")]
    public async Task<ActionResult> BulkGradeSubmissions([FromBody] BulkGradingRequest request)
    {
        try
        {
            if (request?.Calificaciones == null || !request.Calificaciones.Any())
                return BadRequest(new { message = "No hay calificaciones para procesar" });

            _logger.LogInformation($"Calificando {request.Calificaciones.Count} entregas");

            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            var result = await _gradingUnifyService.BulkGradeSubmissionsAsync(
                request.TareaId,
                request.Calificaciones,
                userId);

            return result.Success
                ? Ok(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en calificación bulk");
            return StatusCode(500, new { message = "Error al calificar" });
        }
    }

    /// <summary>
    /// Obtiene calificación consolidada de un estudiante en un curso.
    /// </summary>
    [HttpGet("{alumnoId}/consolidated")]
    public async Task<ActionResult<ConsolidatedGradeDto>> GetConsolidatedGrade(int alumnoId, int cursoId, int periodoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo calificación consolidada para alumno {alumnoId}");
            var grade = await _gradingUnifyService.GetConsolidatedGradeAsync(alumnoId, cursoId, periodoId);
            return Ok(grade);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener calificación consolidada");
            return StatusCode(500, new { message = "Error al obtener calificación" });
        }
    }

    /// <summary>
    /// Obtiene estadísticas de una evaluación en un curso.
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<GradeStatisticsDto>> GetGradeStatistics(int cursoId, int evaluacionId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo estadísticas para evaluación {evaluacionId}");
            var stats = await _gradingUnifyService.GetGradeStatisticsAsync(cursoId, evaluacionId);
            return Ok(stats);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas");
            return StatusCode(500, new { message = "Error al obtener estadísticas" });
        }
    }

    /// <summary>
    /// Actualiza una calificación con auditoría.
    /// </summary>
    [HttpPost("update-grade")]
    public async Task<ActionResult> UpdateGradeWithHistory([FromBody] UpdateGradeRequest request)
    {
        try
        {
            if (request == null || request.EntregaId <= 0)
                return BadRequest(new { message = "Datos inválidos" });

            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            var result = await _gradingUnifyService.UpdateGradeWithHistoryAsync(
                request.EntregaId,
                request.NuevaNota,
                request.Razon ?? "Ajuste de calificación",
                userId);

            return result.Success
                ? Ok(new { message = result.Message })
                : BadRequest(new { message = result.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar calificación");
            return StatusCode(500, new { message = "Error al actualizar calificación" });
        }
    }

    /// <summary>
    /// Obtiene historial de cambios de una calificación.
    /// </summary>
    [HttpGet("audit-trail")]
    public async Task<ActionResult<List<GradeChangeHistoryDto>>> GetAuditTrail(int entregaId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo auditoría para entrega {entregaId}");
            // Aquí se llamaría a AuditTrailService
            return Ok(new List<GradeChangeHistoryDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditoría");
            return StatusCode(500, new { message = "Error al obtener auditoría" });
        }
    }
}

// Request/Response DTOs
public class BulkGradingRequest
{
    public int TareaId { get; set; }
    public List<BulkGradeSubmissionDto> Calificaciones { get; set; } = [];
}

public class UpdateGradeRequest
{
    public int EntregaId { get; set; }
    public decimal NuevaNota { get; set; }
    public string? Razon { get; set; }
}
