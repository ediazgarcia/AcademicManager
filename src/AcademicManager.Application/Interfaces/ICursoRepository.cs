using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface ICursoRepository
{
    Task<Curso?> GetByIdAsync(int id);
    Task<Curso?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Curso>> GetAllAsync();
    Task<IEnumerable<Curso>> GetByGradoIdAsync(int gradoId);
    Task<int> GetTotalCountAsync();
    Task<int> CreateAsync(Curso curso);
    Task<bool> UpdateAsync(Curso curso);
    Task<bool> DeleteAsync(int id);
}
