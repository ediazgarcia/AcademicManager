using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio para gestionar notificaciones de estudiantes.
/// Crea alertas de tareas vencidas, calificaciones nuevas, etc.
/// </summary>
public class NotificationService
{
    private readonly IStudentNotificationRepository _notificationRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly ITareaRepository _tareaRepository;
    private readonly IEntregaTareaRepository _entregaTareaRepository;

    public NotificationService(
        IStudentNotificationRepository notificationRepository,
        IAlumnoRepository alumnoRepository,
        ITareaRepository tareaRepository,
        IEntregaTareaRepository entregaTareaRepository)
    {
        _notificationRepository = notificationRepository;
        _alumnoRepository = alumnoRepository;
        _tareaRepository = tareaRepository;
        _entregaTareaRepository = entregaTareaRepository;
    }

    /// <summary>
    /// Notifica a estudiantes sobre tareas próximas a vencer.
    /// </summary>
    public async Task NotifyTaskDueAsync(List<int> tareaIds, int diasAntesDel Vencimiento = 3)
    {
        foreach (var tareaId in tareaIds)
        {
            var tarea = await _tareaRepository.GetByIdAsync(tareaId);
            if (tarea == null) continue;

            var diasRestantes = (tarea.FechaEntrega - DateTime.UtcNow).Days;
            if (diasRestantes == diasAntesDel Vencimiento)
            {
                // Obtener alumnos del curso
                var entregas = (await _entregaTareaRepository.GetByTareaIdAsync(tareaId))
                    .Select(e => e.AlumnoId)
                    .Distinct()
                    .ToList();

                foreach (var alumnoId in entregas)
                {
                    var notificacion = new StudentNotification
                    {
                        AlumnoId = alumnoId,
                        Tipo = "TaskDue",
                        Titulo = "Tarea próxima a vencer",
                        Contenido = $"La tarea '{tarea.Titulo}' vence en {diasRestantes} días",
                        CreatedAt = DateTime.UtcNow
                    };

                    await _notificationRepository.CreateAsync(notificacion);
                }
            }
        }
    }

    /// <summary>
    /// Notifica a estudiante cuando se publican calificaciones.
    /// </summary>
    public async Task NotifyGradePublishedAsync(int alumnoId, int evaluacionId, string evaluacionNombre)
    {
        var notificacion = new StudentNotification
        {
            AlumnoId = alumnoId,
            Tipo = "GradePublished",
            Titulo = "Nueva calificación disponible",
            Contenido = $"Tu calificación para '{evaluacionNombre}' está disponible",
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = evaluacionId
        };

        await _notificationRepository.CreateAsync(notificacion);
    }

    /// <summary>
    /// Notifica a docente cuando su planificación es rechazada.
    /// </summary>
    public async Task NotifyPlanificacionRejectedAsync(int docenteId, int planId, string motivo)
    {
        var notificacion = new StudentNotification
        {
            AlumnoId = docenteId, // Reutilicemos para notificaciones generales
            Tipo = "PlanificacionRejected",
            Titulo = "Planificación rechazada",
            Contenido = $"Tu planificación ha sido rechazada. Motivo: {motivo}",
            CreatedAt = DateTime.UtcNow,
            RelatedEntityId = planId
        };

        await _notificationRepository.CreateAsync(notificacion);
    }

    /// <summary>
    /// Obtiene resumen de notificaciones pendientes para un usuario.
    /// </summary>
    public async Task<List<NotificacionDto>> GetNotificationsSummaryAsync(int userId)
    {
        var notificaciones = (await _notificationRepository.GetByAlumnoIdAsync(userId))
            .Where(n => !n.Leida)
            .OrderByDescending(n => n.CreatedAt)
            .Take(10)
            .ToList();

        return notificaciones.Select(n => new NotificacionDto
        {
            Id = n.Id,
            Tipo = n.Tipo,
            Titulo = n.Titulo,
            Contenido = n.Contenido,
            FechaCreacion = n.CreatedAt,
            Leida = n.Leida
        }).ToList();
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    public async Task<bool> MarkAsReadAsync(int notificacionId)
    {
        var notificacion = await _notificationRepository.GetByIdAsync(notificacionId);
        if (notificacion == null)
            return false;

        notificacion.Leida = true;
        notificacion.LeidaAt = DateTime.UtcNow;

        return await _notificationRepository.UpdateAsync(notificacion);
    }
}
