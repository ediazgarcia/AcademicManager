using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IEvaluacionRepository
{
    Task<IEnumerable<Evaluacion>> GetByPlanificacionIdAsync(int planificacionId);
    Task<Evaluacion?> GetByIdAsync(int id);
    Task<int> AddAsync(Evaluacion evaluacion);
    Task<bool> UpdateAsync(Evaluacion evaluacion);
    Task<bool> DeleteAsync(int id);
}
