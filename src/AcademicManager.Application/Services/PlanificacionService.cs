using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio de Planificación - Gestiona operaciones de planificaciones con estándares MINERD
/// Implementa validaciones, auditoría y ciclo de aprobaciones
/// </summary>
public class PlanificacionService
{
    private readonly IPlanificacionRepository _planificacionRepository;
    private readonly IDocenteRepository _docenteRepository;
    //private readonly IValidationService _validationService;
    private readonly PlanificacionValidationService _minerdValidator;
    private readonly ILogger<PlanificacionService> _logger;

    public PlanificacionService(
        IPlanificacionRepository planificacionRepository,
        IDocenteRepository docenteRepository,
        //IValidationService validationService,
        PlanificacionValidationService minerdValidator,
        ILogger<PlanificacionService> logger)
    {
        _planificacionRepository = planificacionRepository;
        _docenteRepository = docenteRepository;
        //_validationService = validationService;
        _minerdValidator = minerdValidator;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    // OPERACIONES CRUD
    // ═══════════════════════════════════════════════════════════

    public async Task<Planificacion?> ObtenerPorIdAsync(int id) =>
        await _planificacionRepository.GetByIdAsync(id);

    public async Task<IEnumerable<Planificacion>> ObtenerTodosAsync() =>
        await _planificacionRepository.GetAllAsync();

    public async Task<IEnumerable<Planificacion>> ObtenerPorDocenteAsync(int docenteId) =>
        await _planificacionRepository.GetByDocenteIdAsync(docenteId);

    public async Task<IEnumerable<Planificacion>> ObtenerPorCursoAsync(int cursoId) =>
        await _planificacionRepository.GetByCursoIdAsync(cursoId);

    public async Task<IEnumerable<Planificacion>> ObtenerPorPeriodoAsync(int periodoId) =>
        await _planificacionRepository.GetByPeriodoIdAsync(periodoId);

    public async Task<IEnumerable<Planificacion>> ObtenerPorEstadoAsync(string estado) =>
        await _planificacionRepository.GetByEstadoAsync(estado);

    /// <summary>
    /// Crea una nueva planificación con validaciones MINERD
    /// </summary>
    public async Task<(bool Success, int Id, string Message)> CrearAsync(Planificacion planificacion)
    {
        try
        {
            if (planificacion.DocenteId <= 0)
            {
                return (false, 0, "Debe seleccionar un docente válido.");
            }

            var docente = await _docenteRepository.GetByIdAsync(planificacion.DocenteId);
            if (docente == null)
            {
                return (false, 0, $"El docente seleccionado (ID {planificacion.DocenteId}) no existe. Actualice sesión o seleccione otro docente.");
            }

            // Validar estructura MINERD
            var validacionMinerd = _minerdValidator.ValidarEstructuraMinerd(planificacion);
            if (!validacionMinerd.IsValid)
            {
                return (false, 0, validacionMinerd.ErrorMessage);
            }

            planificacion.FechaCreacion = DateTime.UtcNow;
            planificacion.Estado = "Borrador";

            var id = await _planificacionRepository.CreateAsync(planificacion);

            _logger.LogInformation($"Planificación creada: ID={id}, Docente={planificacion.DocenteId}");

            // Log de advertencias si las hay
            if (validacionMinerd.Warnings.Count > 0)
            {
                _logger.LogWarning($"Advertencias al crear planificación {id}: {string.Join("; ", validacionMinerd.Warnings)}");
            }

            return (true, id, "Planificación creada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al crear planificación: {ex.Message}");
            return (false, 0, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza una planificación existente
    /// </summary>
    public async Task<(bool Success, string Message)> ActualizarAsync(Planificacion planificacion)
    {
        try
        {
            if (planificacion.DocenteId <= 0)
                return (false, "Debe seleccionar un docente válido.");

            var docente = await _docenteRepository.GetByIdAsync(planificacion.DocenteId);
            if (docente == null)
                return (false, $"El docente seleccionado (ID {planificacion.DocenteId}) no existe. Actualice sesión o seleccione otro docente.");

            var planActual = await ObtenerPorIdAsync(planificacion.Id);
            if (planActual == null)
                return (false, "Planificación no encontrada");

            if (planActual.Estado != "Borrador" && planActual.Estado != "Rechazado")
                return (false, $"No se puede modificar planificación en estado '{planActual.Estado}'");

            // Validar estructura MINERD
            var validacionMinerd = _minerdValidator.ValidarEstructuraMinerd(planificacion);
            if (!validacionMinerd.IsValid)
                return (false, validacionMinerd.ErrorMessage);

            planificacion.FechaActualizacion = DateTime.UtcNow;
            var resultado = await _planificacionRepository.UpdateAsync(planificacion);

            if (resultado)
                _logger.LogInformation($"Planificación actualizada: ID={planificacion.Id}");

            return (resultado, resultado ? "Actualizado exitosamente" : "Error al actualizar");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al actualizar planificación: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    public async Task<bool> EliminarAsync(int id)
    {
        try
        {
            var plan = await ObtenerPorIdAsync(id);
            if (plan == null) return false;

            return await _planificacionRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al eliminar planificación: {ex.Message}");
            return false;
        }
    }

    // ═══════════════════════════════════════════════════════════
    // CAMBIOS DE ESTADO Y APROBACIÓN
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Docente envía planificación para revisión
    /// </summary>
    public async Task<(bool Success, string Message)> EnviarParaRevisionAsync(int id, int docenteId)
    {
        try
        {
            var plan = await ObtenerPorIdAsync(id);
            if (plan == null) return (false, "Planificación no encontrada");

            if (plan.DocenteId != docenteId)
                return (false, "No tiene permiso para modificar esta planificación");

            if (plan.Estado != "Borrador" && plan.Estado != "Rechazado")
                return (false, $"No se puede enviar planificación en estado '{plan.Estado}'");

            // Validación MINERD más estricta para envío
            var validacionMinerd = _minerdValidator.ValidarEstructuraMinerd(plan);
            if (!validacionMinerd.IsValid)
                return (false, $"Plantificación incompleta: {validacionMinerd.ErrorMessage}");

            // Generar informe de integridad
            var informe = _minerdValidator.GenerarInformeIntegridad(plan);
            _logger.LogInformation($"Informe de integridad al enviar: {System.Text.Json.JsonSerializer.Serialize(informe)}");

            plan.Estado = "Enviado";
            var resultado = await _planificacionRepository.CambiarEstadoAsync(id, "Enviado");

            if (resultado)
            {
                await RegistrarAuditoriaAsync(id, docenteId, "Enviar", "Borrador", "Enviado", "Planificación enviada para aprobación");
                _logger.LogInformation($"Planificación enviada para revisión: ID={id}");
            }

            return (resultado, resultado ? "Enviado para revisión" : "Error al enviar");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al enviar planificación: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Supervisor/Director aprueba planificación
    /// </summary>
    public async Task<(bool Success, string Message)> AprobarAsync(int id, int aprobadorId)
    {
        try
        {
            var plan = await ObtenerPorIdAsync(id);
            if (plan == null) return (false, "Planificación no encontrada");

            if (plan.Estado != "Enviado")
                return (false, $"Solo se pueden aprobar planificaciones en estado 'Enviado', actual: '{plan.Estado}'");

            plan.Estado = "Aprobado";
            plan.UsuarioAprobadorId = aprobadorId;
            plan.FechaAprobacion = DateTime.UtcNow;

            var resultado = await _planificacionRepository.AprobarAsync(id, aprobadorId);

            if (resultado)
            {
                await RegistrarAuditoriaAsync(id, aprobadorId, "Aprobar", "Enviado", "Aprobado", "Planificación aprobada");
                _logger.LogInformation($"Planificación aprobada: ID={id}, Aprobador={aprobadorId}");
            }

            return (resultado, resultado ? "Aprobado exitosamente" : "Error al aprobar");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al aprobar planificación: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Supervisor/Director rechaza planificación con motivo
    /// </summary>
    public async Task<(bool Success, string Message)> RechazarAsync(int id, int rechazadorId, string motivo)
    {
        try
        {
            var plan = await ObtenerPorIdAsync(id);
            if (plan == null) return (false, "Planificación no encontrada");

            if (string.IsNullOrWhiteSpace(motivo))
                return (false, "Debe especificar un motivo de rechazo");

            plan.Estado = "Rechazado";
            plan.MotivoRechazo = motivo;

            var resultado = await _planificacionRepository.RechazarAsync(id, rechazadorId, motivo);

            if (resultado)
            {
                await RegistrarAuditoriaAsync(id, rechazadorId, "Rechazar", "Enviado", "Rechazado", motivo);
                _logger.LogInformation($"Planificación rechazada: ID={id}, Motivo={motivo}");
            }

            return (resultado, resultado ? "Rechazado" : "Error al rechazar");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al rechazar planificación: {ex.Message}");
            return (false, $"Error: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // BÚSQUEDA Y FILTRADO
    // ═══════════════════════════════════════════════════════════

    public async Task<IEnumerable<Planificacion>> BuscarAsync(string criterio) =>
        await _planificacionRepository.BuscarAsync(criterio);

    public async Task<IEnumerable<Planificacion>> ObtenerPaginadoAsync(int pagina, int tamanoPagina) =>
        await _planificacionRepository.ObtenerPaginadoAsync(pagina, tamanoPagina);

    public async Task<IEnumerable<Planificacion>> ObtenerPendientesAprobacionAsync() =>
        await _planificacionRepository.ObtenerPendientesAprobacionAsync();

    // ═══════════════════════════════════════════════════════════
    // AUDITORÍA
    // ═══════════════════════════════════════════════════════════

    private async Task RegistrarAuditoriaAsync(int planificacionId, int usuarioId, string accion, 
        string estadoAnterior, string estadoNuevo, string observaciones = "")
    {
        try
        {
            var auditoria = new PlanificacionAuditoria
            {
                PlanificacionId = planificacionId,
                UsuarioId = usuarioId,
                Accion = accion,
                EstadoAnterior = estadoAnterior,
                EstadoNuevo = estadoNuevo,
                Observaciones = observaciones,
                FechaAccion = DateTime.UtcNow
            };

            await _planificacionRepository.RegistrarAuditoriaAsync(auditoria);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error al registrar auditoría: {ex.Message}");
        }
    }

    public async Task<IEnumerable<PlanificacionAuditoria>> ObtenerHistorialAsync(int planificacionId) =>
        await _planificacionRepository.ObtenerHistorialAsync(planificacionId);
}
