using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class SeccionService
{
    private readonly ISeccionRepository _seccionRepository;

    public SeccionService(ISeccionRepository seccionRepository)
    {
        _seccionRepository = seccionRepository;
    }

    public Task<Seccion?> ObtenerPorIdAsync(int id) =>
        _seccionRepository.GetByIdAsync(id);

    public Task<IEnumerable<Seccion>> ObtenerTodosAsync() =>
        _seccionRepository.GetAllAsync();

    public Task<IEnumerable<Seccion>> ObtenerPorGradoAsync(int gradoId) =>
        _seccionRepository.GetByGradoIdAsync(gradoId);

    public Task<int> CrearAsync(Seccion seccion) =>
        _seccionRepository.CreateAsync(seccion);

    public Task<bool> ActualizarAsync(Seccion seccion) =>
        _seccionRepository.UpdateAsync(seccion);

    public Task<bool> EliminarAsync(int id) =>
        _seccionRepository.DeleteAsync(id);
}
