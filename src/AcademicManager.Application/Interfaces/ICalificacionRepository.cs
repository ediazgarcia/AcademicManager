using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface ICalificacionRepository
{
    Task<IEnumerable<Calificacion>> GetByEvaluacionIdAsync(int evaluacionId);
    Task<Calificacion?> GetByEvaluacionAndAlumnoAsync(int evaluacionId, int alumnoId);
    Task<int> AddAsync(Calificacion calificacion);
    Task<bool> UpdateAsync(Calificacion calificacion);
    Task<bool> DeleteAsync(int id);
}
