using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class PeriodoAcademicoService
{
    private readonly IPeriodoAcademicoRepository _periodoRepository;

    public PeriodoAcademicoService(IPeriodoAcademicoRepository periodoRepository)
    {
        _periodoRepository = periodoRepository;
    }

    public Task<PeriodoAcademico?> ObtenerPorIdAsync(int id) =>
        _periodoRepository.GetByIdAsync(id);

    public Task<IEnumerable<PeriodoAcademico>> ObtenerTodosAsync() =>
        _periodoRepository.GetAllAsync();

    public Task<PeriodoAcademico?> ObtenerActivoAsync() =>
        _periodoRepository.GetActivoAsync();

    public Task<int> CrearAsync(PeriodoAcademico periodo) =>
        _periodoRepository.CreateAsync(periodo);

    public Task<bool> ActualizarAsync(PeriodoAcademico periodo) =>
        _periodoRepository.UpdateAsync(periodo);

    public Task<bool> EliminarAsync(int id) =>
        _periodoRepository.DeleteAsync(id);
}
