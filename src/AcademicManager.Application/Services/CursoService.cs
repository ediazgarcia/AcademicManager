using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class CursoService
{
    private readonly ICursoRepository _cursoRepository;

    public CursoService(ICursoRepository cursoRepository)
    {
        _cursoRepository = cursoRepository;
    }

    public Task<Curso?> ObtenerPorIdAsync(int id) =>
        _cursoRepository.GetByIdAsync(id);

    public Task<IEnumerable<Curso>> ObtenerTodosAsync() =>
        _cursoRepository.GetAllAsync();

    public Task<IEnumerable<Curso>> ObtenerPorGradoAsync(int gradoId) =>
        _cursoRepository.GetByGradoIdAsync(gradoId);

    public Task<int> ObtenerTotalAsync() =>
        _cursoRepository.GetTotalCountAsync();

    public async Task<int> CrearAsync(Curso curso)
    {
        var existente = await _cursoRepository.GetByCodigoAsync(curso.Codigo);
        if (existente != null)
            throw new InvalidOperationException($"Ya existe un curso con el código '{curso.Codigo}'.");

        return await _cursoRepository.CreateAsync(curso);
    }

    public Task<bool> ActualizarAsync(Curso curso) =>
        _cursoRepository.UpdateAsync(curso);

    public Task<bool> EliminarAsync(int id) =>
        _cursoRepository.DeleteAsync(id);
}
