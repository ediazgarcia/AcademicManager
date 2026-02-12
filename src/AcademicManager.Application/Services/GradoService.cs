using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class GradoService
{
    private readonly IGradoRepository _gradoRepository;

    public GradoService(IGradoRepository gradoRepository)
    {
        _gradoRepository = gradoRepository;
    }

    public Task<Grado?> ObtenerPorIdAsync(int id) =>
        _gradoRepository.GetByIdAsync(id);

    public Task<IEnumerable<Grado>> ObtenerTodosAsync() =>
        _gradoRepository.GetAllAsync();

    public Task<int> CrearAsync(Grado grado) =>
        _gradoRepository.CreateAsync(grado);

    public Task<bool> ActualizarAsync(Grado grado) =>
        _gradoRepository.UpdateAsync(grado);

    public Task<bool> EliminarAsync(int id) =>
        _gradoRepository.DeleteAsync(id);
}
