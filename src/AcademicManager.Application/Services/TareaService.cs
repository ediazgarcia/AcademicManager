using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class TareaService
{
    private readonly ITareaRepository _tareaRepository;
    private readonly IEntregaTareaRepository _entregaTareaRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IHorarioRepository _horarioRepository;

    public TareaService(ITareaRepository tareaRepository, IEntregaTareaRepository entregaTareaRepository, IAlumnoRepository alumnoRepository, IHorarioRepository horarioRepository)
    {
        _tareaRepository = tareaRepository;
        _entregaTareaRepository = entregaTareaRepository;
        _alumnoRepository = alumnoRepository;
        _horarioRepository = horarioRepository;
    }

    public Task<Tarea?> ObtenerPorIdAsync(int id) => _tareaRepository.GetByIdAsync(id);

    public Task<IEnumerable<Tarea>> ObtenerTodosAsync() => _tareaRepository.GetAllAsync();

    public Task<IEnumerable<Tarea>> ObtenerPorDocenteAsync(int docenteId) => _tareaRepository.GetByDocenteIdAsync(docenteId);

    public Task<IEnumerable<Tarea>> ObtenerPorCursoAsync(int cursoId) => _tareaRepository.GetByCursoIdAsync(cursoId);

    public Task<IEnumerable<Tarea>> ObtenerPorPeriodoAsync(int periodoId) => _tareaRepository.GetByPeriodoIdAsync(periodoId);

    public Task<IEnumerable<Tarea>> ObtenerPorAlumnoAsync(int alumnold, int periodoId) => _tareaRepository.GetByAlumnoIdAsync(alumnold, periodoId);

    public async Task<int> CrearAsync(Tarea tarea)
    {
        tarea.FechaCreacion = DateTime.UtcNow;
        return await _tareaRepository.CreateAsync(tarea);
    }

    public Task<bool> ActualizarAsync(Tarea tarea)
    {
        tarea.FechaActualizacion = DateTime.UtcNow;
        return _tareaRepository.UpdateAsync(tarea);
    }

    public Task<bool> EliminarAsync(int id) => _tareaRepository.DeleteAsync(id);

    public Task<EntregaTarea?> ObtenerEntregaPorTareaYAlumnoAsync(int tareaId, int alumnold) 
        => _entregaTareaRepository.GetByTareaAndAlumnoAsync(tareaId, alumnold);

    public Task<IEnumerable<EntregaTarea>> ObtenerEntregasPorTareaAsync(int tareaId) 
        => _entregaTareaRepository.GetByTareaIdAsync(tareaId);

    public Task<IEnumerable<EntregaTarea>> ObtenerEntregasPorAlumnoAsync(int alumnold) 
        => _entregaTareaRepository.GetByAlumnoIdAsync(alumnold);

    public Task<EntregaTarea?> ObtenerEntregaAsync(int entregaId)
        => _entregaTareaRepository.GetByIdAsync(entregaId);

    public async Task<int> CrearEntregaAsync(EntregaTarea entrega, Tarea tarea)
    {
        // Verificar que el alumno exista y esté asignado a una sección/cursos del período
        var alumno = await _alumnoRepository.GetByIdAsync(entrega.AlumnoId);
        if (alumno == null)
            throw new InvalidOperationException("Alumno no encontrado.");

        if (!alumno.SeccionId.HasValue)
            throw new InvalidOperationException("El alumno no está asignado a una sección.");

        // Validar que el curso de la tarea esté en el horario de la sección para el mismo periodo
        var horariosSeccion = (await _horarioRepository.GetBySeccionIdAsync(alumno.SeccionId.Value)).ToList();
        var existeHorario = horariosSeccion.Any(h => h.PeriodoAcademicoId == tarea.PeriodoAcademicoId && h.CursoId == tarea.CursoId);
        if (!existeHorario)
            throw new UnauthorizedAccessException("No tienes permiso para entregar esta tarea.");

        var existente = await _entregaTareaRepository.GetByTareaAndAlumnoAsync(entrega.TareaId, entrega.AlumnoId);
        if (existente != null)
        {
            throw new InvalidOperationException("Ya has enviado una entrega para esta tarea.");
        }

        entrega.EsTardia = DateTime.UtcNow > tarea.FechaEntrega;
        entrega.FechaEntrega = DateTime.UtcNow;
        
        return await _entregaTareaRepository.CreateAsync(entrega);
    }

    public async Task<bool> CalificarEntregaAsync(int entregaId, int puntos, string? retroalimentacion)
    {
        var entrega = await _entregaTareaRepository.GetByIdAsync(entregaId);
        if (entrega == null)
            return false;

        var tarea = await _tareaRepository.GetByIdAsync(entrega.TareaId);
        if (tarea == null || puntos > tarea.PuntosMaximos)
            return false;

        entrega.Puntos = puntos;
        entrega.Retroalimentacion = retroalimentacion;
        entrega.FechaCalificacion = DateTime.UtcNow;
        entrega.FechaActualizacion = DateTime.UtcNow;

        return await _entregaTareaRepository.UpdateAsync(entrega);
    }

    public async Task<(int puntosObtenidos, int puntosMaximos, double porcentaje)> GetCalificacionPeriodoAsync(int alumnold, int periodoId)
    {
        var puntosObtenidos = await _entregaTareaRepository.GetTotalPuntosByAlumnoAndPeriodoAsync(alumnold, periodoId);
        var puntosMaximos = await _entregaTareaRepository.GetTotalPuntosMaximosByAlumnoAndPeriodoAsync(alumnold, periodoId);
        var porcentaje = puntosMaximos > 0 ? (double)puntosObtenidos / puntosMaximos * 100 : 0;

        return (puntosObtenidos, puntosMaximos, Math.Round(porcentaje, 2));
    }
}
