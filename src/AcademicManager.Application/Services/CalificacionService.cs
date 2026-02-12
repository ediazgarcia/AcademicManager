using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class CalificacionService
{
    private readonly ICalificacionRepository _repository;

    public CalificacionService(ICalificacionRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Calificacion>> GetByEvaluacionIdAsync(int evaluacionId) => _repository.GetByEvaluacionIdAsync(evaluacionId);
    public Task<Calificacion?> GetByEvaluacionAndAlumnoAsync(int evaluacionId, int alumnoId) => _repository.GetByEvaluacionAndAlumnoAsync(evaluacionId, alumnoId);
    public Task<int> CreateAsync(Calificacion calificacion) => _repository.AddAsync(calificacion);
    public Task<bool> UpdateAsync(Calificacion calificacion) => _repository.UpdateAsync(calificacion);
    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}
