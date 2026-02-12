using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IHorarioRepository
{
    Task<Horario?> GetByIdAsync(int id);
    Task<IEnumerable<Horario>> GetAllAsync();
    Task<IEnumerable<Horario>> GetByDocenteIdAsync(int docenteId);
    Task<IEnumerable<Horario>> GetBySeccionIdAsync(int seccionId);
    Task<IEnumerable<Horario>> GetByPeriodoIdAsync(int periodoId);
    Task<int> CreateAsync(Horario horario);
    Task<bool> UpdateAsync(Horario horario);
    Task<bool> DeleteAsync(int id);
}
