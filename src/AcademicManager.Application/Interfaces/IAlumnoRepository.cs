using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IAlumnoRepository
{
    Task<Alumno?> GetByIdAsync(int id);
    Task<Alumno?> GetByCodigoAsync(string codigo);
    Task<IEnumerable<Alumno>> GetAllAsync();
    Task<IEnumerable<Alumno>> GetByGradoIdAsync(int gradoId);
    Task<IEnumerable<Alumno>> GetBySeccionIdAsync(int seccionId);
    Task<int> GetTotalCountAsync();
    Task<int> CreateAsync(Alumno alumno);
    Task<bool> UpdateAsync(Alumno alumno);
    Task<bool> DeleteAsync(int id);
}
