using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface ITareaRepository
{
    Task<Tarea?> GetByIdAsync(int id);
    Task<IEnumerable<Tarea>> GetAllAsync();
    Task<IEnumerable<Tarea>> GetByDocenteIdAsync(int docenteId);
    Task<IEnumerable<Tarea>> GetByCursoIdAsync(int cursoId);
    Task<IEnumerable<Tarea>> GetByPeriodoIdAsync(int periodoId);
    Task<IEnumerable<Tarea>> GetByAlumnoIdAsync(int alumnold, int periodoId);
    Task<int> CreateAsync(Tarea tarea);
    Task<bool> UpdateAsync(Tarea tarea);
    Task<bool> DeleteAsync(int id);
}

public interface IEntregaTareaRepository
{
    Task<EntregaTarea?> GetByIdAsync(int id);
    Task<EntregaTarea?> GetByTareaAndAlumnoAsync(int tareaId, int alumnold);
    Task<IEnumerable<EntregaTarea>> GetByTareaIdAsync(int tareaId);
    Task<IEnumerable<EntregaTarea>> GetByAlumnoIdAsync(int alumnold);
    Task<int> CreateAsync(EntregaTarea entrega);
    Task<bool> UpdateAsync(EntregaTarea entrega);
    Task<bool> DeleteAsync(int id);
    Task<int> GetTotalPuntosByAlumnoAndPeriodoAsync(int alumnold, int periodoId);
    Task<int> GetTotalPuntosMaximosByAlumnoAndPeriodoAsync(int alumnold, int periodoId);
}
