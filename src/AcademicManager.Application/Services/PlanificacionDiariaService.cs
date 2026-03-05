using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio de Planificación Diaria - Gestiona los planes de clase diarios.
/// Estándares MINERD: Intención pedagógica + 3 momentos de la clase (Inicio, Desarrollo, Cierre).
/// </summary>
public class PlanificacionDiariaService
{
    private readonly IPlanificacionDiariaRepository _repository;
    private readonly ILogger<PlanificacionDiariaService> _logger;

    public PlanificacionDiariaService(
        IPlanificacionDiariaRepository repository,
        ILogger<PlanificacionDiariaService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    // CONSULTAS
    // ═══════════════════════════════════════════════════════════

    public async Task<PlanificacionDiaria?> ObtenerPorIdAsync(int id) =>
        await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<PlanificacionDiaria>> ObtenerTodosAsync() =>
        await _repository.GetAllAsync();

    /// <summary>
    /// Obtiene todos los planes diarios de una planificación mensual.
    /// </summary>
    public async Task<IEnumerable<PlanificacionDiaria>> ObtenerPorPlanMensualAsync(int planMensualId) =>
        await _repository.GetByPlanificacionMensualIdAsync(planMensualId);

    // ═══════════════════════════════════════════════════════════
    // CRUD
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Crea un nuevo plan diario dentro de un plan mensual.
    /// Valida los campos obligatorios del estándar MINERD.
    /// </summary>
    public async Task<(bool Success, int Id, string Message)> CrearAsync(PlanificacionDiaria plan)
    {
        try
        {
            if (plan.PlanificacionMensualId <= 0)
                return (false, 0, "Debe asociar a una planificación mensual.");

            if (plan.Fecha == default)
                return (false, 0, "La fecha de la clase es obligatoria.");

            if (string.IsNullOrWhiteSpace(plan.IntencionPedagogica))
                return (false, 0, "La intención pedagógica es obligatoria.");

            // Validar tiempos mínimos (MINERD recomienda distribución)
            var tiempoTotal = plan.TiempoInicioMinutos + plan.TiempoDesarrolloMinutos + plan.TiempoCierreMinutos;
            if (tiempoTotal <= 0)
                return (false, 0, "Los tiempos de la clase deben ser mayores a cero.");

            plan.FechaCreacion = DateTime.UtcNow;
            plan.Estado = "Borrador";

            var id = await _repository.CreateAsync(plan);
            _logger.LogInformation("Plan diario creado: ID={Id}, Fecha={Fecha}, MensualId={MensualId}",
                id, plan.Fecha.ToShortDateString(), plan.PlanificacionMensualId);

            return (true, id, "Plan diario creado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear plan diario");
            return (false, 0, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un plan diario existente.
    /// </summary>
    public async Task<(bool Success, string Message)> ActualizarAsync(PlanificacionDiaria plan)
    {
        try
        {
            var actual = await _repository.GetByIdAsync(plan.Id);
            if (actual == null)
                return (false, "Plan diario no encontrado.");

            if (actual.Estado == "Aprobado")
                return (false, "No se puede modificar un plan diario aprobado.");

            plan.FechaActualizacion = DateTime.UtcNow;
            var resultado = await _repository.UpdateAsync(plan);

            if (resultado)
                _logger.LogInformation("Plan diario actualizado: ID={Id}", plan.Id);

            return (resultado, resultado ? "Actualizado exitosamente." : "Error al actualizar.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar plan diario ID={Id}", plan.Id);
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina un plan diario (solo si está en Borrador).
    /// </summary>
    public async Task<(bool Success, string Message)> EliminarAsync(int id)
    {
        try
        {
            var plan = await _repository.GetByIdAsync(id);
            if (plan == null)
                return (false, "Plan diario no encontrado.");

            var resultado = await _repository.DeleteAsync(id);
            return (resultado, resultado ? "Eliminado exitosamente." : "Error al eliminar.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar plan diario ID={Id}", id);
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Cambia el estado de un plan diario.
    /// </summary>
    public async Task<(bool Success, string Message)> CambiarEstadoAsync(int id, string nuevoEstado)
    {
        try
        {
            var plan = await _repository.GetByIdAsync(id);
            if (plan == null)
                return (false, "Plan diario no encontrado.");

            var resultado = await _repository.CambiarEstadoAsync(id, nuevoEstado);
            return (resultado, resultado ? $"Estado cambiado a {nuevoEstado}." : "Error al cambiar estado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del plan diario ID={Id}", id);
            return (false, $"Error: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // UTILIDADES
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Calcula el tiempo total distribuido en una clase (Inicio + Desarrollo + Cierre).
    /// </summary>
    public static int CalcularTiempoTotal(PlanificacionDiaria plan) =>
        plan.TiempoInicioMinutos + plan.TiempoDesarrolloMinutos + plan.TiempoCierreMinutos;

    /// <summary>
    /// Retorna el porcentaje de distribución del tiempo siguiendo recomendaciones MINERD.
    /// Inicio: 15-20%, Desarrollo: 60-70%, Cierre: 15-20%
    /// </summary>
    public static (int PctInicio, int PctDesarrollo, int PctCierre) CalcularDistribucionTiempo(PlanificacionDiaria plan)
    {
        var total = CalcularTiempoTotal(plan);
        if (total == 0) return (0, 0, 0);
        return (
            (int)Math.Round((double)plan.TiempoInicioMinutos / total * 100),
            (int)Math.Round((double)plan.TiempoDesarrolloMinutos / total * 100),
            (int)Math.Round((double)plan.TiempoCierreMinutos / total * 100)
        );
    }
}
