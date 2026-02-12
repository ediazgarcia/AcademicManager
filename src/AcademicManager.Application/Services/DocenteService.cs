using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class DocenteService
{
    private readonly IDocenteRepository _docenteRepository;

    public DocenteService(IDocenteRepository docenteRepository)
    {
        _docenteRepository = docenteRepository;
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

    public Task<bool> EliminarAsync(int id) =>
        _docenteRepository.DeleteAsync(id);
}
