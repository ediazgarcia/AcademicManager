namespace AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

public interface IPlanificacionMensualRepository : IGenericRepository<PlanificacionMensual>
{
    Task<IEnumerable<PlanificacionMensual>> GetByDocenteIdAsync(int docenteId);
    Task<IEnumerable<PlanificacionMensual>> GetByCursoIdAsync(int cursoId);
    Task<IEnumerable<PlanificacionMensual>> GetByPlanificacionAnualIdAsync(int planificacionAnualId);
    Task<PlanificacionMensual?> GetByPlanificacionAnualIdAndMesAsync(int planificacionAnualId, int numeroMes);
    Task<bool> CambiarEstadoAsync(int id, string estado);
}
