using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IDocenteRepository
{
    Task<Docente?> GetByIdAsync(int id);
    Task<Docente?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Docente>> GetAllAsync();
    Task<int> GetTotalCountAsync();
    Task<int> CreateAsync(Docente docente);
    Task<bool> UpdateAsync(Docente docente);
    Task<bool> DeleteAsync(int id);
}
