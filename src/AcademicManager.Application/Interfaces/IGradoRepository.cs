using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IGradoRepository
{
    Task<Grado?> GetByIdAsync(int id);
    Task<IEnumerable<Grado>> GetAllAsync();
    Task<int> CreateAsync(Grado grado);
    Task<bool> UpdateAsync(Grado grado);
    Task<bool> DeleteAsync(int id);
}
