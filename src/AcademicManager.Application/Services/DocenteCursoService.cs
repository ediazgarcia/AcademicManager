using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class DocenteCursoService
{
    private readonly IDocenteCursoRepository _docenteCursoRepository;
    private readonly IDocenteRepository _docenteRepository;
    private readonly ICursoRepository _cursoRepository;

    public DocenteCursoService(
        IDocenteCursoRepository docenteCursoRepository,
        IDocenteRepository docenteRepository,
        ICursoRepository cursoRepository)
    {
        _docenteCursoRepository = docenteCursoRepository;
        _docenteRepository = docenteRepository;
        _cursoRepository = cursoRepository;
    }

    public Task<IEnumerable<DocenteCurso>> ObtenerAsignacionesPorDocenteAsync(int docenteId) =>
        _docenteCursoRepository.GetByDocenteIdAsync(docenteId);

    public async Task<IEnumerable<Curso>> ObtenerCursosPorDocenteAsync(int docenteId)
    {
        var asignaciones = (await _docenteCursoRepository.GetByDocenteIdAsync(docenteId)).ToList();
        if (!asignaciones.Any())
        {
            return [];
        }

        var cursos = new List<Curso>();
        foreach (var cursoId in asignaciones.Select(a => a.CursoId).Distinct())
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso != null && curso.Activo)
            {
                cursos.Add(curso);
            }
        }

        return cursos.OrderBy(c => c.Nombre).ToList();
    }

    public async Task<int> AsignarDocenteAsync(int docenteId, int cursoId)
    {
        var docente = await _docenteRepository.GetByIdAsync(docenteId);
        if (docente == null)
        {
            throw new InvalidOperationException("El docente no existe.");
        }

        var curso = await _cursoRepository.GetByIdAsync(cursoId);
        if (curso == null || !curso.Activo)
        {
            throw new InvalidOperationException("El curso no existe o está inactivo.");
        }

        var alreadyExists = await _docenteCursoRepository.ExistsAsync(docenteId, cursoId);
        if (alreadyExists)
        {
            throw new InvalidOperationException("El docente ya está asignado a este curso.");
        }

        var asignacion = new DocenteCurso
        {
            DocenteId = docenteId,
            CursoId = cursoId,
            FechaAsignacion = DateTime.UtcNow,
            Activo = true
        };

        return await _docenteCursoRepository.CreateAsync(asignacion);
    }

    public Task<bool> DesasignarDocenteAsync(int docenteId, int cursoId) =>
        _docenteCursoRepository.DeleteByDocenteAndCursoAsync(docenteId, cursoId);
}
