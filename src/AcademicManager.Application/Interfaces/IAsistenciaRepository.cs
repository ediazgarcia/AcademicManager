using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IAsistenciaRepository
{
    Task<IEnumerable<Asistencia>> GetByPlanificacionIdAndDateAsync(int planificacionId, DateTime fecha);
    Task<IEnumerable<Asistencia>> GetByPlanificacionIdAsync(int planificacionId);
    Task<int> AddAsync(Asistencia asistencia);
    Task<bool> UpdateAsync(Asistencia asistencia);
    Task<bool> DeleteAsync(int id);
}
