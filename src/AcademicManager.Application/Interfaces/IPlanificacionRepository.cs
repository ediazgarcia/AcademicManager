using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

/// <summary>
/// Interfaz para operaciones de Planificación
/// Implementa patrones MINERD para gestión profesional de planificaciones
/// </summary>
public interface IPlanificacionRepository
{
    // CRUD Básico
    Task<Planificacion?> GetByIdAsync(int id);
    Task<IEnumerable<Planificacion>> GetAllAsync();
    Task<IEnumerable<Planificacion>> GetByDocenteIdAsync(int docenteId);
    Task<IEnumerable<Planificacion>> GetByCursoIdAsync(int cursoId);
    Task<IEnumerable<Planificacion>> GetByPeriodoIdAsync(int periodoId);
    Task<IEnumerable<Planificacion>> GetByEstadoAsync(string estado);
    Task<int> CreateAsync(Planificacion planificacion);
    Task<bool> UpdateAsync(Planificacion planificacion);
    Task<bool> DeleteAsync(int id);

    // Cambios de Estado
    Task<bool> CambiarEstadoAsync(int id, string estado);
    Task<bool> AprobarAsync(int id, int aprobadorId);
    Task<bool> RechazarAsync(int id, int rechazadorId, string motivo);

    // Búsqueda y Filtrado
    Task<IEnumerable<Planificacion>> BuscarAsync(string criterio);
    Task<IEnumerable<Planificacion>> ObtenerPaginadoAsync(int pagina, int tamanoPagina);
    Task<IEnumerable<Planificacion>> ObtenerPendientesAprobacionAsync();

    // Auditoría
    Task<IEnumerable<PlanificacionAuditoria>> ObtenerHistorialAsync(int planificacionId);
    Task<bool> RegistrarAuditoriaAsync(PlanificacionAuditoria auditoria);
}
