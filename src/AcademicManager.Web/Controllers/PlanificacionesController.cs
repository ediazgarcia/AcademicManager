using AcademicManager.Application.Services;
using AcademicManager.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace AcademicManager.Web.Controllers;

/// <summary>
/// API REST para gestión de Planificaciones con estándares MINERD
/// Endpoints para CRUD, aprobación, auditoría y exportación de documentos
/// </summary>
[ApiController]
[Route("api/planificaciones")]
[Produces("application/json")]
public class PlanificacionesController : ControllerBase
{
    private readonly PlanificacionService _planificacionService;
    private readonly ILogger<PlanificacionesController> _logger;

    public PlanificacionesController(
        PlanificacionService planificacionService,
        ILogger<PlanificacionesController> logger)
    {
        _planificacionService = planificacionService;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    // GET - OBTENER
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Obtiene todas las planificaciones
    /// </summary>
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<Planificacion>>> ObtenerTodas()
    {
        var planificaciones = await _planificacionService.ObtenerTodosAsync();
        return Ok(planificaciones);
    }

    /// <summary>
    /// Obtiene una planificación específica por ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<Planificacion>> ObtenerPorId(int id)
    {
        var planificacion = await _planificacionService.ObtenerPorIdAsync(id);
        if (planificacion == null)
            return NotFound(new { message = "Planificación no encontrada" });

        return Ok(planificacion);
    }

    /// <summary>
    /// Obtiene planificaciones de un docente específico
    /// </summary>
    [HttpGet("docente/{docenteId}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<Planificacion>>> ObtenerPorDocente(int docenteId)
    {
        var planificaciones = await _planificacionService.ObtenerPorDocenteAsync(docenteId);
        return Ok(planificaciones);
    }

    /// <summary>
    /// Obtiene planificaciones de un curso específico
    /// </summary>
    [HttpGet("curso/{cursoId}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<Planificacion>>> ObtenerPorCurso(int cursoId)
    {
        var planificaciones = await _planificacionService.ObtenerPorCursoAsync(cursoId);
        return Ok(planificaciones);
    }

    /// <summary>
    /// Obtiene planificaciones por período académico
    /// </summary>
    [HttpGet("periodo/{periodoId}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<Planificacion>>> ObtenerPorPeriodo(int periodoId)
    {
        var planificaciones = await _planificacionService.ObtenerPorPeriodoAsync(periodoId);
        return Ok(planificaciones);
    }

    /// <summary>
    /// Obtiene planificaciones por estado
    /// </summary>
    [HttpGet("estado/{estado}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<Planificacion>>> ObtenerPorEstado(string estado)
    {
        var planificaciones = await _planificacionService.ObtenerPorEstadoAsync(estado);
        return Ok(planificaciones);
    }

    /// <summary>
    /// Obtiene planificaciones pendientes de aprobación
    /// </summary>
    [HttpGet("pendientes-aprobacion")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<Planificacion>>> ObtenerPendientesAprobacion()
    {
        var planificaciones = await _planificacionService.ObtenerPendientesAprobacionAsync();
        return Ok(planificaciones);
    }

    /// <summary>
    /// Busca planificaciones por criterio
    /// </summary>
    [HttpGet("buscar/{criterio}")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<Planificacion>>> Buscar(string criterio)
    {
        var resultados = await _planificacionService.BuscarAsync(criterio);
        return Ok(resultados);
    }

    // ═══════════════════════════════════════════════════════════
    // POST - CREAR
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Crea una nueva planificación
    /// </summary>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<object>> Crear([FromBody] Planificacion planificacion)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var resultado = await _planificacionService.CrearAsync(planificacion);
        if (!resultado.Success)
            return BadRequest(new { message = resultado.Message });

        return CreatedAtAction(nameof(ObtenerPorId), new { id = resultado.Id },
            new { id = resultado.Id, message = resultado.Message });
    }

    // ═══════════════════════════════════════════════════════════
    // PUT - ACTUALIZAR
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Actualiza una planificación existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<object>> Actualizar(int id, [FromBody] Planificacion planificacion)
    {
        if (id != planificacion.Id)
            return BadRequest(new { message = "ID no coincide" });

        var resultado = await _planificacionService.ActualizarAsync(planificacion);
        if (!resultado.Success)
            return BadRequest(new { message = resultado.Message });

        return Ok(new { message = resultado.Message });
    }

    // ═══════════════════════════════════════════════════════════
    // DELETE - ELIMINAR
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Elimina una planificación (solo si está en estado Borrador)
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    public async Task<ActionResult> Eliminar(int id)
    {
        var resultado = await _planificacionService.EliminarAsync(id);
        if (!resultado)
            return BadRequest(new { message = "No se puede eliminar una planificación que no esté en estado Borrador" });

        return NoContent();
    }

    // ═══════════════════════════════════════════════════════════
    // CAMBIOS DE ESTADO
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Docente envía planificación para revisión
    /// </summary>
    [HttpPost("{id}/enviar-revision")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<object>> EnviarParaRevision(int id, [FromQuery] int docenteId)
    {
        var resultado = await _planificacionService.EnviarParaRevisionAsync(id, docenteId);
        if (!resultado.Success)
            return BadRequest(new { message = resultado.Message });

        return Ok(new { message = resultado.Message });
    }

    /// <summary>
    /// Supervisor/Director aprueba una planificación
    /// </summary>
    [HttpPost("{id}/aprobar")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<object>> Aprobar(int id, [FromQuery] int aprobadorId)
    {
        var resultado = await _planificacionService.AprobarAsync(id, aprobadorId);
        if (!resultado.Success)
            return BadRequest(new { message = resultado.Message });

        return Ok(new { message = resultado.Message });
    }

    /// <summary>
    /// Supervisor/Director rechaza una planificación
    /// </summary>
    [HttpPost("{id}/rechazar")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<object>> Rechazar(int id, [FromBody] RechazarRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            return BadRequest(new { message = "Debe especificar un motivo de rechazo" });

        var resultado = await _planificacionService.RechazarAsync(id, request.RechazadorId, request.Motivo);
        if (!resultado.Success)
            return BadRequest(new { message = resultado.Message });

        return Ok(new { message = resultado.Message });
    }

    // ═══════════════════════════════════════════════════════════
    // AUDITORÍA E HISTORIAL
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Obtiene el historial de cambios de una planificación
    /// </summary>
    [HttpGet("{id}/historial")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<IEnumerable<PlanificacionAuditoria>>> ObtenerHistorial(int id)
    {
        var historial = await _planificacionService.ObtenerHistorialAsync(id);
        return Ok(historial);
    }

    // ═══════════════════════════════════════════════════════════
    // MODELOS DE SOLICITUD
    // ═══════════════════════════════════════════════════════════

    public class RechazarRequest
    {
        [Required]
        public int RechazadorId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string Motivo { get; set; } = string.Empty;
    }
}
