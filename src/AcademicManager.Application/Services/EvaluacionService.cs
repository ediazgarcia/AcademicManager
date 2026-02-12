using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class EvaluacionService
{
    private readonly IEvaluacionRepository _repository;

    public EvaluacionService(IEvaluacionRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Evaluacion>> GetByPlanificacionIdAsync(int planificacionId) => _repository.GetByPlanificacionIdAsync(planificacionId);
    public Task<Evaluacion?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);
    public Task<int> CreateAsync(Evaluacion evaluacion) => _repository.AddAsync(evaluacion);
    public Task<bool> UpdateAsync(Evaluacion evaluacion) => _repository.UpdateAsync(evaluacion);
    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}
