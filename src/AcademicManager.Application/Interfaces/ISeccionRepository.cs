using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface ISeccionRepository
{
    Task<Seccion?> GetByIdAsync(int id);
    Task<IEnumerable<Seccion>> GetAllAsync();
    Task<IEnumerable<Seccion>> GetByGradoIdAsync(int gradoId);
    Task<int> CreateAsync(Seccion seccion);
    Task<bool> UpdateAsync(Seccion seccion);
    Task<bool> DeleteAsync(int id);
}
