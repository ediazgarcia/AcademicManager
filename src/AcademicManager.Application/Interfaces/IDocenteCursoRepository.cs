using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IDocenteCursoRepository
{
    Task<DocenteCurso?> GetByIdAsync(int id);
    Task<IEnumerable<DocenteCurso>> GetByDocenteIdAsync(int docenteId);
    Task<IEnumerable<DocenteCurso>> GetByCursoIdAsync(int cursoId);
    Task<bool> ExistsAsync(int docenteId, int cursoId);
    Task<int> CreateAsync(DocenteCurso asignacion);
    Task<bool> DeleteByDocenteAndCursoAsync(int docenteId, int cursoId);
    Task<int> DeleteByDocenteAsync(int docenteId);
}
