using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio para analizar y reportar progreso académico de estudiantes.
/// Proporciona dashboards, tendencias y alertas de riesgo académico.
/// </summary>
public class StudentProgressService
{
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly ITareaRepository _tareaRepository;
    private readonly IEntregaTareaRepository _entregaTareaRepository;
    private readonly IEvaluacionRepository _evaluacionRepository;
    private readonly ICalificacionRepository _calificacionRepository;
    private readonly IMatriculaCursoRepository _matriculaCursoRepository;
    private readonly ICursoRepository _cursoRepository;
    private readonly IDocenteRepository _docenteRepository;
    private readonly GradingUnifyService _gradingUnifyService;

    public StudentProgressService(
        IAlumnoRepository alumnoRepository,
        ITareaRepository tareaRepository,
        IEntregaTareaRepository entregaTareaRepository,
        IEvaluacionRepository evaluacionRepository,
        ICalificacionRepository calificacionRepository,
        IMatriculaCursoRepository matriculaCursoRepository,
        ICursoRepository cursoRepository,
        IDocenteRepository docenteRepository,
        GradingUnifyService gradingUnifyService)
    {
        _alumnoRepository = alumnoRepository;
        _tareaRepository = tareaRepository;
        _entregaTareaRepository = entregaTareaRepository;
        _evaluacionRepository = evaluacionRepository;
        _calificacionRepository = calificacionRepository;
        _matriculaCursoRepository = matriculaCursoRepository;
        _cursoRepository = cursoRepository;
        _docenteRepository = docenteRepository;
        _gradingUnifyService = gradingUnifyService;
    }

    /// <summary>
    /// Obtiene el dashboard completo de progreso de un estudiante.
    /// </summary>
    public async Task<StudentProgressDashboardDto> GetProgressDashboardAsync(int alumnoId, int periodoId)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId)
            ?? throw new InvalidOperationException("Alumno no encontrado.");

        var matriculas = (await _matriculaCursoRepository.GetByAlumnoIdAsync(alumnoId))
            .Where(m => m.Activo)
            .ToList();

        var cursos = new List<CourseProgressDto>();
        decimal sumaNotas = 0;
        int cursosCalificados = 0;

        foreach (var matricula in matriculas)
        {
            var curso = await _cursoRepository.GetByIdAsync(matricula.CursoId)
                ?? throw new InvalidOperationException($"Curso {matricula.CursoId} no encontrado.");

            var gradoConsolidado = await _gradingUnifyService.GetConsolidatedGradeAsync(alumnoId, matricula.CursoId, periodoId);

            sumaNotas += gradoConsolidado.NotaFinal;
            cursosCalificados++;

            var tareas = (await _tareaRepository.GetByCursoIdAsync(matricula.CursoId))
                .Where(t => t.PeriodoAcademicoId == periodoId)
                .ToList();

            var tareasEntregadas = 0;
            var tareasPendientes = 0;
            var tareasVencidas = 0;

            foreach (var tarea in tareas)
            {
                if (tarea.FechaEntrega < DateTime.UtcNow && tarea.Activa)
                    tareasVencidas++;
                else
                {
                    var entrega = await _entregaTareaRepository.GetByTareaAndAlumnoAsync(tarea.Id, alumnoId);
                    if (entrega != null)
                        tareasEntregadas++;
                    else
                        tareasPendientes++;
                }
            }

            var evaluaciones = (await _evaluacionRepository.GetByCursoIdAsync(matricula.CursoId))
                .Select(e => new EvaluacionProgressDto
                {
                    EvaluacionId = e.Id,
                    Nombre = e.Nombre,
                    Peso = e.Peso
                }).ToList();

            var riesgo = DetermineRiskIndicator(gradoConsolidado.NotaFinal);

            cursos.Add(new CourseProgressDto
            {
                CursoId = matricula.CursoId,
                NombreCurso = curso.Nombre,
                DocenteId = 0, // Obtener del schedule si aplica
                NombreDocente = "Docente",
                NotaActual = gradoConsolidado.NotaFinal,
                PromedioClase = 75m, // Calcular dinámicamente
                TareasEntregadas = tareasEntregadas,
                TareasPendientes = tareasPendientes,
                TareasVencidas = tareasVencidas,
                RiesgoAcademico = riesgo,
                Evaluaciones = evaluaciones,
                Literal = gradoConsolidado.Literal
            });
        }

        var promedioPeriodo = cursosCalificados > 0 ? sumaNotas / cursosCalificados : 0;
        var riesgoPeriodo = DetermineRiskIndicator(promedioPeriodo);

        return new StudentProgressDashboardDto
        {
            AlumnoId = alumnoId,
            NombreAlumno = $"{alumno.Nombres} {alumno.Apellidos}",
            GradoNombre = "Grado", // Complementar con repositorio
            SeccionNombre = "Sección", // Complementar
            PeriodoId = periodoId,
            NombrePeriodo = "Período Actual",
            CursosPorCurso = cursos,
            PromedioPeriodo = promedioPeriodo,
            RiesgoAcademico = riesgoPeriodo,
            CursosAprobados = cursos.Count(c => c.NotaActual >= 60),
            CursosDesaprobados = cursos.Count(c => c.NotaActual < 60)
        };
    }

    /// <summary>
    /// Obtiene tendencias de desempeño del estudiante a través de períodos.
    /// </summary>
    public async Task<StudentPerformanceTrendDto> GetPerformanceTrendAsync(int alumnoId, int ultimosPeriodos = 4)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId)
            ?? throw new InvalidOperationException("Alumno no encontrado.");

        // Aquí debería obtener datos históricos de la BD
        // Por ahora retornamos estructura vacía
        var tendencias = new List<PerformanceTrendDto>();

        return new StudentPerformanceTrendDto
        {
            AlumnoId = alumnoId,
            NombreAlumno = $"{alumno.Nombres} {alumno.Apellidos}",
            Tendencias = tendencias,
            TendenciaGlobal = 0m
        };
    }

    /// <summary>
    /// Obtiene indicador de riesgo académico del estudiante.
    /// </summary>
    public async Task<RiskIndicatorReportDto> GetRiskIndicatorAsync(int alumnoId, int periodoId)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId)
            ?? throw new InvalidOperationException("Alumno no encontrado.");

        var dashboard = await GetProgressDashboardAsync(alumnoId, periodoId);

        var cursosEnRiesgo = dashboard.CursosPorCurso
            .Where(c => c.NotaActual < 60)
            .Count();

        var cursosConBajaNota = dashboard.CursosPorCurso
            .Where(c => c.NotaActual < 70)
            .Select(c => c.NombreCurso)
            .ToList();

        var tareasVencidas = dashboard.CursosPorCurso
            .Sum(c => c.TareasVencidas);

        var recomendacion = GenerateAcademicRecommendation(dashboard.PromedioPeriodo, cursosEnRiesgo, tareasVencidas);

        return new RiskIndicatorReportDto
        {
            AlumnoId = alumnoId,
            NombreAlumno = $"{alumno.Nombres} {alumno.Apellidos}",
            IndicadorRiesgo = dashboard.RiesgoAcademico,
            CursosEnRiesgo = cursosEnRiesgo,
            CursosConBajaNota = cursosConBajaNota,
            TareasVencidas = tareasVencidas,
            Recomendacion = recomendacion
        };
    }

    /// <summary>
    /// Compara desempeño del estudiante con el promedio de clase.
    /// </summary>
    public async Task<StudentPeerComparisonDto> CompareToPeerAsync(int alumnoId, int cursoId)
    {
        // Obtener nota del estudiante
        var matriculasAlumno = await _matriculaCursoRepository.GetByAlumnoIdAsync(alumnoId);
        var estaEnCurso = matriculasAlumno.Any(m => m.CursoId == cursoId && m.Activo);

        if (!estaEnCurso)
            throw new InvalidOperationException("Estudiante no está matriculado en este curso.");

        // Calcular promedio de clase (simplificado)
        var promedioClase = 75m;

        var notaEstudiante = 80m; // Debería obtener del GradingUnifyService

        var diferencia = notaEstudiante - promedioClase;

        var cuartil = notaEstudiante >= 75 ? "Q1" : notaEstudiante >= 60 ? "Q2" : "Q3";

        return new StudentPeerComparisonDto
        {
            AlumnoId = alumnoId,
            CursoId = cursoId,
            NotaEstudiante = notaEstudiante,
            PromedioClase = promedioClase,
            Diferencia = diferencia,
            PosicionEnClase = 5,
            TotalEstudiantes = 30,
            CuartilPertenencia = cuartil
        };
    }

    /// <summary>
    /// Genera recomendaciones personalizadas de mejora académica.
    /// </summary>
    public async Task<List<StudentRecommendationDto>> GenerateRecommendationsAsync(int alumnoId, int cursoId)
    {
        var recomendaciones = new List<StudentRecommendationDto>();

        // Lógica de recomendaciones basada en desempeño
        var comparacion = await CompareToPeerAsync(alumnoId, cursoId);

        if (comparacion.Diferencia < -10)
        {
            recomendaciones.Add(new StudentRecommendationDto
            {
                AlumnoId = alumnoId,
                CursoId = cursoId,
                NombreCurso = "Curso",
                NotaActual = comparacion.NotaEstudiante,
                NotaPromedioPorcentaje = comparacion.PromedioClase,
                Recomendacion = "Tu desempeño está por debajo del promedio. Se recomenda buscar tutorías.",
                NecesitaAyuda = true
            });
        }

        return recomendaciones;
    }

    /// <summary>
    /// Obtiene desglose de calificaciones por curso.
    /// </summary>
    public async Task<CourseBudgetDto> GetCourseGradeBreakdownAsync(int alumnoId, int cursoId, int periodoId)
    {
        var consolidado = await _gradingUnifyService.GetConsolidatedGradeAsync(alumnoId, cursoId, periodoId);

        return new CourseBudgetDto
        {
            CursoId = cursoId,
            NombreCurso = consolidado.NombreCurso,
            NotaTarea = consolidado.NotaTareas,
            NotaEvaluaciones = consolidado.NotaEvaluaciones,
            NotaFinal = consolidado.NotaFinal,
            Componentes = consolidado.Componentes
        };
    }

    // Helper Methods

    private static RiskIndicatorEnum DetermineRiskIndicator(decimal nota)
    {
        return nota switch
        {
            >= 70 => RiskIndicatorEnum.Verde,
            >= 60 => RiskIndicatorEnum.Amarillo,
            _ => RiskIndicatorEnum.Rojo
        };
    }

    private static string GenerateAcademicRecommendation(decimal promedio, int cursosEnRiesgo, int tareasVencidas)
    {
        if (promedio < 60 && cursosEnRiesgo > 2)
            return "CRÍTICO: Necesita intervención inmediata. Contacte con coordinador.";

        if (cursosEnRiesgo > 0 || tareasVencidas > 0)
            return $"En riesgo en {cursosEnRiesgo} curso(s) y {tareasVencidas} tarea(s) vencida(s). Se recomienda refuerzo académico.";

        return promedio < 70
            ? "Desempeño regular. Puede mejorar con dedicación."
            : "Desempeño satisfactorio. Continúe así.";
    }
}
