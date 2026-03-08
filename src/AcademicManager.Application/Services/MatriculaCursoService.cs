using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class MatriculaCursoService
{
    private readonly IMatriculaCursoRepository _matriculaCursoRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly ICursoRepository _cursoRepository;

    public MatriculaCursoService(
        IMatriculaCursoRepository matriculaCursoRepository,
        IAlumnoRepository alumnoRepository,
        ICursoRepository cursoRepository)
    {
        _matriculaCursoRepository = matriculaCursoRepository;
        _alumnoRepository = alumnoRepository;
        _cursoRepository = cursoRepository;
    }

    public Task<IEnumerable<MatriculaCurso>> ObtenerMatriculasPorAlumnoAsync(int alumnoId) =>
        _matriculaCursoRepository.GetByAlumnoIdAsync(alumnoId);

    public async Task<IEnumerable<Curso>> ObtenerCursosPorAlumnoAsync(int alumnoId)
    {
        var matriculas = (await _matriculaCursoRepository.GetByAlumnoIdAsync(alumnoId)).ToList();
        if (!matriculas.Any())
        {
            return [];
        }

        var cursos = new List<Curso>();
        foreach (var cursoId in matriculas.Select(m => m.CursoId).Distinct())
        {
            var curso = await _cursoRepository.GetByIdAsync(cursoId);
            if (curso != null && curso.Activo)
            {
                cursos.Add(curso);
            }
        }

        return cursos.OrderBy(c => c.Nombre).ToList();
    }

    public async Task<int> MatricularAlumnoAsync(int alumnoId, int cursoId)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId);
        if (alumno == null)
        {
            throw new InvalidOperationException("El alumno no existe.");
        }

        var curso = await _cursoRepository.GetByIdAsync(cursoId);
        if (curso == null || !curso.Activo)
        {
            throw new InvalidOperationException("El curso no existe o está inactivo.");
        }

        var alreadyExists = await _matriculaCursoRepository.ExistsAsync(alumnoId, cursoId);
        if (alreadyExists)
        {
            throw new InvalidOperationException("El alumno ya está matriculado en este curso.");
        }

        var matricula = new MatriculaCurso
        {
            AlumnoId = alumnoId,
            CursoId = cursoId,
            FechaMatricula = DateTime.UtcNow,
            Activo = true
        };

        return await _matriculaCursoRepository.CreateAsync(matricula);
    }

    public Task<bool> DesmatricularAlumnoAsync(int alumnoId, int cursoId) =>
        _matriculaCursoRepository.DeleteByAlumnoAndCursoAsync(alumnoId, cursoId);
}
