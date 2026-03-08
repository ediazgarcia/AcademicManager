using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio de Planificación Mensual - Gestiona planes mensuales derivados de la planificación anual.
/// Estándares MINERD: Unidad didáctica mensual con competencias, contenidos y evaluación.
/// </summary>
public class PlanificacionMensualService
{
    private readonly IPlanificacionMensualRepository _repository;
    private readonly IDocenteRepository _docenteRepository;
    private readonly ILogger<PlanificacionMensualService> _logger;

    // Meses del año lectivo dominicano (Agosto - Junio)
    public static readonly (int Numero, string Nombre)[] MesesLectivos =
    [
        (8, "Agosto"), (9, "Septiembre"), (10, "Octubre"), (11, "Noviembre"),
        (12, "Diciembre"), (1, "Enero"), (2, "Febrero"), (3, "Marzo"),
        (4, "Abril"), (5, "Mayo"), (6, "Junio")
    ];

    public PlanificacionMensualService(
        IPlanificacionMensualRepository repository,
        IDocenteRepository docenteRepository,
        ILogger<PlanificacionMensualService> logger)
    {
        _repository = repository;
        _docenteRepository = docenteRepository;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════
    // CONSULTAS
    // ═══════════════════════════════════════════════════════════

    public async Task<PlanificacionMensual?> ObtenerPorIdAsync(int id) =>
        await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<PlanificacionMensual>> ObtenerTodosAsync() =>
        await _repository.GetAllAsync();

    public async Task<IEnumerable<PlanificacionMensual>> ObtenerPorDocenteAsync(int docenteId) =>
        await _repository.GetByDocenteIdAsync(docenteId);

    public async Task<IEnumerable<PlanificacionMensual>> ObtenerPorCursoAsync(int cursoId) =>
        await _repository.GetByCursoIdAsync(cursoId);

    /// <summary>
    /// Obtiene todos los planes mensuales de una planificación anual específica.
    /// Los devuelve ordenados por mes (NumeroMes ASC).
    /// </summary>
    public async Task<IEnumerable<PlanificacionMensual>> ObtenerPorPlanAnualAsync(int planAnualId) =>
        await _repository.GetByPlanificacionAnualIdAsync(planAnualId);

    /// <summary>
    /// Verifica si ya existe un plan mensual para un mes específico dentro del plan anual.
    /// Útil para impedir duplicados (1 plan mensual por mes por anual).
    /// </summary>
    public async Task<PlanificacionMensual?> ObtenerPorAnualYMesAsync(int planAnualId, int numeroMes) =>
        await _repository.GetByPlanificacionAnualIdAndMesAsync(planAnualId, numeroMes);

    // ═══════════════════════════════════════════════════════════
    // CRUD
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Crea un nuevo plan mensual derivado de un plan anual.
    /// Valida que no exista ya un plan para ese mes en ese plan anual.
    /// </summary>
    public async Task<(bool Success, int Id, string Message)> CrearAsync(PlanificacionMensual plan)
    {
        try
        {
            if (plan.DocenteId <= 0)
                return (false, 0, "Debe seleccionar un docente válido para la planificación mensual.");

            var docente = await _docenteRepository.GetByIdAsync(plan.DocenteId);
            if (docente == null)
                return (false, 0, $"El docente asociado (ID {plan.DocenteId}) no existe.");

            // Validar campos obligatorios
            if (plan.PlanificacionId <= 0)
                return (false, 0, "Debe asociar a una planificación anual.");

            if (plan.NumeroMes < 1 || plan.NumeroMes > 12)
                return (false, 0, "El número de mes no es válido.");

            if (string.IsNullOrWhiteSpace(plan.TituloUnidad))
                return (false, 0, "El título de la unidad es obligatorio.");

            // Verificar que no exista ya un plan para ese mes en ese anual
            var existente = await _repository.GetByPlanificacionAnualIdAndMesAsync(plan.PlanificacionId, plan.NumeroMes);
            if (existente != null)
                return (false, 0, $"Ya existe un plan mensual para {plan.Mes} en esta planificación anual.");

            plan.FechaCreacion = DateTime.UtcNow;
            plan.Estado = "Borrador";

            // Asignar nombre del mes si no está seteado
            if (string.IsNullOrWhiteSpace(plan.Mes))
            {
                var mesInfo = MesesLectivos.FirstOrDefault(m => m.Numero == plan.NumeroMes);
                plan.Mes = mesInfo.Nombre ?? $"Mes {plan.NumeroMes}";
            }

            var id = await _repository.CreateAsync(plan);
            _logger.LogInformation("Plan mensual creado: ID={Id}, Mes={Mes}, PlanAnual={AnualId}", id, plan.Mes, plan.PlanificacionId);

            return (true, id, "Plan mensual creado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear plan mensual");
            return (false, 0, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Actualiza un plan mensual existente.
    /// </summary>
    public async Task<(bool Success, string Message)> ActualizarAsync(PlanificacionMensual plan)
    {
        try
        {
            if (plan.DocenteId <= 0)
                return (false, "Debe seleccionar un docente válido para la planificación mensual.");

            var docente = await _docenteRepository.GetByIdAsync(plan.DocenteId);
            if (docente == null)
                return (false, $"El docente asociado (ID {plan.DocenteId}) no existe.");

            var actual = await _repository.GetByIdAsync(plan.Id);
            if (actual == null)
                return (false, "Plan mensual no encontrado.");

            if (actual.Estado == "Aprobado")
                return (false, "No se puede modificar un plan mensual aprobado.");

            plan.FechaActualizacion = DateTime.UtcNow;
            var resultado = await _repository.UpdateAsync(plan);

            if (resultado)
                _logger.LogInformation("Plan mensual actualizado: ID={Id}", plan.Id);

            return (resultado, resultado ? "Actualizado exitosamente." : "Error al actualizar.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar plan mensual ID={Id}", plan.Id);
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Elimina un plan mensual (solo si está en Borrador).
    /// </summary>
    public async Task<(bool Success, string Message)> EliminarAsync(int id)
    {
        try
        {
            var plan = await _repository.GetByIdAsync(id);
            if (plan == null)
                return (false, "Plan mensual no encontrado.");

            var resultado = await _repository.DeleteAsync(id);
            return (resultado, resultado ? "Eliminado exitosamente." : "Error al eliminar.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar plan mensual ID={Id}", id);
            return (false, $"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Cambia el estado de un plan mensual.
    /// </summary>
    public async Task<(bool Success, string Message)> CambiarEstadoAsync(int id, string nuevoEstado)
    {
        try
        {
            var plan = await _repository.GetByIdAsync(id);
            if (plan == null)
                return (false, "Plan mensual no encontrado.");

            var resultado = await _repository.CambiarEstadoAsync(id, nuevoEstado);
            return (resultado, resultado ? $"Estado cambiado a {nuevoEstado}." : "Error al cambiar estado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del plan mensual ID={Id}", id);
            return (false, $"Error: {ex.Message}");
        }
    }

    // ═══════════════════════════════════════════════════════════
    // UTILIDADES
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Obtiene la vista de progreso mensual de un plan anual (qué meses tienen plan y cuáles no).
    /// </summary>
    public async Task<List<(int NumeroMes, string NombreMes, PlanificacionMensual? Plan)>> ObtenerProgresoMensualAsync(int planAnualId)
    {
        var planesExistentes = (await _repository.GetByPlanificacionAnualIdAsync(planAnualId)).ToList();
        var resultado = new List<(int, string, PlanificacionMensual?)>();

        foreach (var (numero, nombre) in MesesLectivos)
        {
            var plan = planesExistentes.FirstOrDefault(p => p.NumeroMes == numero);
            resultado.Add((numero, nombre, plan));
        }

        return resultado;
    }
}
