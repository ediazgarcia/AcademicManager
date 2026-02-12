using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IPlanificacionRepository
{
    Task<Planificacion?> GetByIdAsync(int id);
    Task<IEnumerable<Planificacion>> GetAllAsync();
    Task<IEnumerable<Planificacion>> GetByDocenteIdAsync(int docenteId);
    Task<IEnumerable<Planificacion>> GetByCursoIdAsync(int cursoId);
    Task<IEnumerable<Planificacion>> GetByPeriodoIdAsync(int periodoId);
    Task<int> CreateAsync(Planificacion planificacion);
    Task<bool> UpdateAsync(Planificacion planificacion);
    Task<bool> DeleteAsync(int id);
    Task<bool> CambiarEstadoAsync(int id, string estado);
}
