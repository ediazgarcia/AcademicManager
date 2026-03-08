using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IMatriculaCursoRepository
{
    Task<MatriculaCurso?> GetByIdAsync(int id);
    Task<IEnumerable<MatriculaCurso>> GetByAlumnoIdAsync(int alumnoId);
    Task<IEnumerable<MatriculaCurso>> GetByCursoIdAsync(int cursoId);
    Task<bool> ExistsAsync(int alumnoId, int cursoId);
    Task<int> CreateAsync(MatriculaCurso matricula);
    Task<bool> DeleteByAlumnoAndCursoAsync(int alumnoId, int cursoId);
    Task<int> DeleteByAlumnoAsync(int alumnoId);
}
