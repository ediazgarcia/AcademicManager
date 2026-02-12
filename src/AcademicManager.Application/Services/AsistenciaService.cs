using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class AsistenciaService
{
    private readonly IAsistenciaRepository _repository;

    public AsistenciaService(IAsistenciaRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Asistencia>> GetByPlanificacionIdAndDateAsync(int planificacionId, DateTime fecha) => _repository.GetByPlanificacionIdAndDateAsync(planificacionId, fecha);
    public Task<IEnumerable<Asistencia>> GetByPlanificacionIdAsync(int planificacionId) => _repository.GetByPlanificacionIdAsync(planificacionId);
    public Task<int> CreateAsync(Asistencia asistencia) => _repository.AddAsync(asistencia);
    public Task<bool> UpdateAsync(Asistencia asistencia) => _repository.UpdateAsync(asistencia);
    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}
