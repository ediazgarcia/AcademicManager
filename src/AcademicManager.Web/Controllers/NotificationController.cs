using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application.Services;
using AcademicManager.Application.DTOs;

namespace AcademicManager.Web.Controllers;

/// <summary>
/// Controller para endpoints de notificaciones del estudiante.
/// Proporciona acceso a notificaciones pendientes, marca como leídas, etc.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly NotificationService _notificationService;
    private readonly ILogger<NotificationController> _logger;

    public NotificationController(
        NotificationService notificationService,
        ILogger<NotificationController> logger)
    {
        _notificationService = notificationService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene resumen de notificaciones pendientes para el usuario.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<List<NotificacionDto>>> GetNotificationsSummary()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            _logger.LogInformation($"Obteniendo propuestas de notificaciones para usuario {userId}");
            var notifications = await _notificationService.GetNotificationsSummaryAsync(userId);
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones");
            return StatusCode(500, new { message = "Error al obtener notificaciones" });
        }
    }

    /// <summary>
    /// Obtiene todas las notificaciones con filtros.
    /// </summary>
    [HttpGet("")]
    public async Task<ActionResult<List<NotificacionDto>>> GetNotifications(
        string? tipo = null,
        bool? leidas = false,
        int? ultimoN = 20)
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            _logger.LogInformation($"Obteniendo notificaciones filtradas para usuario {userId}");

            // Aquí se llamaría al servicio con filtros
            // Por ahora retornamos lista vacía
            return Ok(new List<NotificacionDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener notificaciones");
            return StatusCode(500, new { message = "Error al obtener notificaciones" });
        }
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    [HttpPost("{notificacionId}/mark-as-read")]
    public async Task<ActionResult> MarkAsRead(int notificacionId)
    {
        try
        {
            _logger.LogInformation($"Marcando notificación {notificacionId} como leída");
            var success = await _notificationService.MarkAsReadAsync(notificacionId);

            if (!success)
                return NotFound(new { message = "Notificación no encontrada" });

            return Ok(new { message = "Notificación marcada como leída" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar como leída");
            return StatusCode(500, new { message = "Error al marcar como leída" });
        }
    }

    /// <summary>
    /// Elimina una notificación.
    /// </summary>
    [HttpDelete("{notificacionId}")]
    public async Task<ActionResult> DeleteNotification(int notificacionId)
    {
        try
        {
            _logger.LogInformation($"Eliminando notificación {notificacionId}");
            // Implementar eliminación en repositorio
            return Ok(new { message = "Notificación eliminada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar notificación");
            return StatusCode(500, new { message = "Error al eliminar notificación" });
        }
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas.
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            _logger.LogInformation($"Obteniendo conteo de notificaciones no leídas para usuario {userId}");
            var notifications = await _notificationService.GetNotificationsSummaryAsync(userId);
            var unreadCount = notifications.Count(n => !n.Leida);

            return Ok(new { unreadCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener conteo");
            return StatusCode(500, new { message = "Error al obtener conteo" });
        }
    }

    /// <summary>
    /// Marca todas las notificaciones como leídas.
    /// </summary>
    [HttpPost("mark-all-as-read")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        try
        {
            var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
            if (userId == 0)
                return Unauthorized(new { message = "Usuario no identificado" });

            _logger.LogInformation($"Marcando todas las notificaciones como leídas para usuario {userId}");
            // Implementar en servicio
            return Ok(new { message = "Todas las notificaciones marcadas como leídas" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar todas como leídas");
            return StatusCode(500, new { message = "Error al marcar como leídas" });
        }
    }
}
