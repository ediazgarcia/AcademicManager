using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class DocenteService
{
    private readonly IDocenteRepository _docenteRepository;
    private readonly IDocenteCursoRepository _docenteCursoRepository;

    public DocenteService(
        IDocenteRepository docenteRepository,
        IDocenteCursoRepository docenteCursoRepository)
    {
        _docenteRepository = docenteRepository;
        _docenteCursoRepository = docenteCursoRepository;
    }

    public Task<Docente?> ObtenerPorIdAsync(int id) =>
        _docenteRepository.GetByIdAsync(id);

    public Task<IEnumerable<Docente>> ObtenerTodosAsync() =>
        _docenteRepository.GetAllAsync();

    public Task<int> ObtenerTotalAsync() =>
        _docenteRepository.GetTotalCountAsync();

    public async Task<int> CrearAsync(Docente docente)
    {
        var existente = await _docenteRepository.GetByCodigoAsync(docente.Codigo);
        if (existente != null)
            throw new InvalidOperationException($"Ya existe un docente con el código '{docente.Codigo}'.");

        return await _docenteRepository.CreateAsync(docente);
    }

    public Task<bool> ActualizarAsync(Docente docente) =>
        _docenteRepository.UpdateAsync(docente);

    public async Task<bool> EliminarAsync(int id)
    {
        await _docenteCursoRepository.DeleteByDocenteAsync(id);
        return await _docenteRepository.DeleteAsync(id);
    }
}
