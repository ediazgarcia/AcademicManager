using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class PlanificacionService
{
    private readonly IPlanificacionRepository _planificacionRepository;

    public PlanificacionService(IPlanificacionRepository planificacionRepository)
    {
        _planificacionRepository = planificacionRepository;
    }

    public Task<Planificacion?> ObtenerPorIdAsync(int id) =>
        _planificacionRepository.GetByIdAsync(id);

    public Task<IEnumerable<Planificacion>> ObtenerTodosAsync() =>
        _planificacionRepository.GetAllAsync();

    public Task<IEnumerable<Planificacion>> ObtenerPorDocenteAsync(int docenteId) =>
        _planificacionRepository.GetByDocenteIdAsync(docenteId);

    public Task<IEnumerable<Planificacion>> ObtenerPorCursoAsync(int cursoId) =>
        _planificacionRepository.GetByCursoIdAsync(cursoId);

    public Task<IEnumerable<Planificacion>> ObtenerPorPeriodoAsync(int periodoId) =>
        _planificacionRepository.GetByPeriodoIdAsync(periodoId);

    public Task<int> CrearAsync(Planificacion planificacion) =>
        _planificacionRepository.CreateAsync(planificacion);

    public Task<bool> ActualizarAsync(Planificacion planificacion)
    {
        planificacion.FechaActualizacion = DateTime.UtcNow;
        return _planificacionRepository.UpdateAsync(planificacion);
    }

    public Task<bool> EliminarAsync(int id) =>
        _planificacionRepository.DeleteAsync(id);

    public Task<bool> EnviarParaRevisionAsync(int id) =>
        _planificacionRepository.CambiarEstadoAsync(id, "Enviado");

    public Task<bool> AprobarAsync(int id) =>
        _planificacionRepository.CambiarEstadoAsync(id, "Aprobado");
}
