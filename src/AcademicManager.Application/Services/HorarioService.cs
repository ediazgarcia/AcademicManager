using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class HorarioService
{
    private readonly IHorarioRepository _horarioRepository;

    public HorarioService(IHorarioRepository horarioRepository)
    {
        _horarioRepository = horarioRepository;
    }

    public Task<Horario?> ObtenerPorIdAsync(int id) =>
        _horarioRepository.GetByIdAsync(id);

    public Task<IEnumerable<Horario>> ObtenerTodosAsync() =>
        _horarioRepository.GetAllAsync();

    public Task<IEnumerable<Horario>> ObtenerPorDocenteAsync(int docenteId) =>
        _horarioRepository.GetByDocenteIdAsync(docenteId);

    public Task<IEnumerable<Horario>> ObtenerPorSeccionAsync(int seccionId) =>
        _horarioRepository.GetBySeccionIdAsync(seccionId);

    public Task<IEnumerable<Horario>> ObtenerPorPeriodoAsync(int periodoId) =>
        _horarioRepository.GetByPeriodoIdAsync(periodoId);

    public Task<int> CrearAsync(Horario horario) =>
        _horarioRepository.CreateAsync(horario);

    public Task<bool> ActualizarAsync(Horario horario) =>
        _horarioRepository.UpdateAsync(horario);

    public Task<bool> EliminarAsync(int id) =>
        _horarioRepository.DeleteAsync(id);
}
