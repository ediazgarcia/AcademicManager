namespace AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

public interface IPlanificacionDiariaRepository : IGenericRepository<PlanificacionDiaria>
{
    Task<IEnumerable<PlanificacionDiaria>> GetByPlanificacionMensualIdAsync(int planificacionMensualId);
    Task<bool> CambiarEstadoAsync(int id, string estado);
}
