using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class AlumnoService
{
    private readonly IAlumnoRepository _alumnoRepository;

    public AlumnoService(IAlumnoRepository alumnoRepository)
    {
        _alumnoRepository = alumnoRepository;
    }

    public Task<Alumno?> ObtenerPorIdAsync(int id) =>
        _alumnoRepository.GetByIdAsync(id);

    public Task<IEnumerable<Alumno>> ObtenerTodosAsync() =>
        _alumnoRepository.GetAllAsync();

    public Task<IEnumerable<Alumno>> ObtenerPorGradoAsync(int gradoId) =>
        _alumnoRepository.GetByGradoIdAsync(gradoId);

    public Task<IEnumerable<Alumno>> ObtenerPorSeccionAsync(int seccionId) =>
        _alumnoRepository.GetBySeccionIdAsync(seccionId);

    public Task<int> ObtenerTotalAsync() =>
        _alumnoRepository.GetTotalCountAsync();

    public async Task<int> CrearAsync(Alumno alumno)
    {
        // Validar código único
        var existente = await _alumnoRepository.GetByCodigoAsync(alumno.Codigo);
        if (existente != null)
            throw new InvalidOperationException($"Ya existe un alumno con el código '{alumno.Codigo}'.");

        return await _alumnoRepository.CreateAsync(alumno);
    }

    public Task<bool> ActualizarAsync(Alumno alumno) =>
        _alumnoRepository.UpdateAsync(alumno);

    public Task<bool> EliminarAsync(int id) =>
        _alumnoRepository.DeleteAsync(id);
}
