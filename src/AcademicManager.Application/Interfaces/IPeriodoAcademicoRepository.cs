using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IPeriodoAcademicoRepository
{
    Task<PeriodoAcademico?> GetByIdAsync(int id);
    Task<IEnumerable<PeriodoAcademico>> GetAllAsync();
    Task<PeriodoAcademico?> GetActivoAsync();
    Task<int> CreateAsync(PeriodoAcademico periodo);
    Task<bool> UpdateAsync(PeriodoAcademico periodo);
    Task<bool> DeleteAsync(int id);
}
