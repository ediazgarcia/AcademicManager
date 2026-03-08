using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio para unificar y consolidar calificaciones de tareas y evaluaciones.
/// Proporciona vista integrada de calificaciones y permite calificación en lote.
/// </summary>
public class GradingUnifyService
{
    private readonly ITareaRepository _tareaRepository;
    private readonly IEntregaTareaRepository _entregaTareaRepository;
    private readonly IEvaluacionRepository _evaluacionRepository;
    private readonly ICalificacionRepository _calificacionRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IGradeAuditTrailRepository _auditTrailRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public GradingUnifyService(
        ITareaRepository tareaRepository,
        IEntregaTareaRepository entregaTareaRepository,
        IEvaluacionRepository evaluacionRepository,
        ICalificacionRepository calificacionRepository,
        IAlumnoRepository alumnoRepository,
        IGradeAuditTrailRepository auditTrailRepository,
        IUsuarioRepository usuarioRepository)
    {
        _tareaRepository = tareaRepository;
        _entregaTareaRepository = entregaTareaRepository;
        _evaluacionRepository = evaluacionRepository;
        _calificacionRepository = calificacionRepository;
        _alumnoRepository = alumnoRepository;
        _auditTrailRepository = auditTrailRepository;
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Obtiene la vista de calificación en lote para una tarea específica.
    /// </summary>
    public async Task<BulkGradingViewDto> GetBulkGradingViewAsync(int tareaId)
    {
        var tarea = await _tareaRepository.GetByIdAsync(tareaId)
            ?? throw new InvalidOperationException("Tarea no encontrada.");

        var entregas = (await _entregaTareaRepository.GetByTareaIdAsync(tareaId))
            .ToList();

        var entries = new List<BulkGradeEntryDto>();
        foreach (var entrega in entregas)
        {
            var alumno = await _alumnoRepository.GetByIdAsync(entrega.AlumnoId);
            entries.Add(new BulkGradeEntryDto
            {
                EntregaTareaId = entrega.Id,
                AlumnoId = entrega.AlumnoId,
                NombreAlumno = alumno != null ? $"{alumno.Nombres} {alumno.Apellidos}" : "N/A",
                NotaActual = entrega.Puntos,
                FechaEntrega = entrega.FechaEntrega,
                EsTardia = entrega.EsTardia,
                Estado = entrega.Puntos.HasValue ? "Calificada" : (entrega.EsTardia ? "Tardía" : "Pendiente")
            });
        }

        return new BulkGradingViewDto
        {
            TareaId = tareaId,
            TareaTitulo = tarea.Titulo,
            CursoId = tarea.CursoId,
            CursoNombre = "Curso", // Será complementado con repositorio si necesario
            PuntosMaximos = tarea.PuntosMaximos,
            FechaEntrega = tarea.FechaEntrega,
            Entregas = entries.OrderBy(e => e.NombreAlumno).ToList(),
            TotalEntregas = entries.Count,
            EntregasCalificadas = entries.Count(e => e.NotaActual.HasValue),
            EntregasPendientes = entries.Count(e => !e.NotaActual.HasValue)
        };
    }

    /// <summary>
    /// Califica múltiples entregas de una tarea a la vez (bulk grading).
    /// </summary>
    public async Task<(bool Success, string Message)> BulkGradeSubmissionsAsync(
        int tareaId,
        List<BulkGradeSubmissionDto> calificaciones,
        int docenteId)
    {
        if (calificaciones == null || calificaciones.Count == 0)
            return (false, "No hay calificaciones para procesar.");

        var tarea = await _tareaRepository.GetByIdAsync(tareaId)
            ?? throw new InvalidOperationException("Tarea no encontrada.");

        var resultados = 0;
        var errores = new List<string>();

        foreach (var calif in calificaciones)
        {
            try
            {
                var entrega = await _entregaTareaRepository.GetByIdAsync(calif.EntregaTareaId)
                    ?? throw new InvalidOperationException($"Entrega {calif.EntregaTareaId} no encontrada.");

                // Registrar cambio en auditoría
                await _auditTrailRepository.CreateAsync(new GradeAuditTrail
                {
                    EntregaTareaId = calif.EntregaTareaId,
                    DocenteId = docenteId,
                    NotaAnterior = entrega.Puntos,
                    NotaNueva = (decimal)calif.Nota,
                    Razon = "Calificación en lote",
                    Timestamp = DateTime.UtcNow
                });

                // Actualizar entrega
                entrega.Puntos = (int)calif.Nota;
                entrega.Retroalimentacion = calif.Retroalimentacion;
                entrega.FechaCalificacion = DateTime.UtcNow;

                await _entregaTareaRepository.UpdateAsync(entrega);
                resultados++;
            }
            catch (Exception ex)
            {
                errores.Add($"Error en entrega {calif.EntregaTareaId}: {ex.Message}");
            }
        }

        var mensaje = $"Calificadas {resultados} entregas";
        if (errores.Count > 0)
            mensaje += $". Errores: {string.Join(", ", errores)}";

        return (errores.Count == 0, mensaje);
    }

    /// <summary>
    /// Obtiene la calificación consolidada de un estudiante en un curso.
    /// Integra notas de tareas y evaluaciones.
    /// </summary>
    public async Task<ConsolidatedGradeDto> GetConsolidatedGradeAsync(
        int alumnoId,
        int cursoId,
        int periodoId)
    {
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId)
            ?? throw new InvalidOperationException("Alumno no encontrado.");

        // Obtener tareas del curso/período
        var tareas = (await _tareaRepository.GetByCursoIdAsync(cursoId))
            .Where(t => t.PeriodoAcademicoId == periodoId)
            .ToList();

        // Calcular nota de tareas
        decimal notaTareas = 0;
        var componentesTareas = new List<GradeComponentDto>();

        if (tareas.Count > 0)
        {
            var puntosObtenidos = 0;
            var puntosMaximos = 0;

            foreach (var tarea in tareas)
            {
                var entrega = await _entregaTareaRepository
                    .GetByTareaAndAlumnoAsync(tarea.Id, alumnoId);

                if (entrega?.Puntos.HasValue == true)
                {
                    puntosObtenidos += entrega.Puntos.Value;
                    puntosMaximos += tarea.PuntosMaximos;

                    componentesTareas.Add(new GradeComponentDto
                    {
                        Tipo = "Tarea",
                        Nombre = tarea.Titulo,
                        Nota = entrega.Puntos.Value,
                        Peso = 100m
                    });
                }
            }

            notaTareas = puntosMaximos > 0 ? (puntosObtenidos / (decimal)puntosMaximos) * 100m : 0;
        }

        // Obtener evaluaciones del curso
        var evaluaciones = (await _evaluacionRepository.GetByCursoIdAsync(cursoId))
            .ToList();

        // Calcular nota de evaluaciones
        decimal notaEvaluaciones = 0;
        var componentesEvaluaciones = new List<GradeComponentDto>();

        if (evaluaciones.Count > 0)
        {
            var notasTotal = 0m;
            var pesosTotal = 0m;

            foreach (var evaluacion in evaluaciones)
            {
                var calificacion = await _calificacionRepository
                    .GetByEvaluacionAndAlumnoAsync(evaluacion.Id, alumnoId);

                if (calificacion != null)
                {
                    var notaConPeso = calificacion.Nota * (evaluacion.Peso / 100m);
                    notasTotal += notaConPeso;
                    pesosTotal += evaluacion.Peso;

                    componentesEvaluaciones.Add(new GradeComponentDto
                    {
                        Tipo = "Evaluacion",
                        Nombre = evaluacion.Nombre,
                        Nota = calificacion.Nota,
                        Peso = evaluacion.Peso,
                        NotaPonderada = notaConPeso
                    });
                }
            }

            notaEvaluaciones = pesosTotal > 0 ? (notasTotal / pesosTotal) * 100m : 0;
        }

        // Calcular nota final (promedio ponderado)
        decimal pesoPorcentajeTareas = 50m;
        decimal pesoPorcentajeEvaluaciones = 50m;

        decimal notaFinal = ((notaTareas * pesoPorcentajeTareas) + (notaEvaluaciones * pesoPorcentajeEvaluaciones)) / 100m;

        var literal = CalculateGradeLiteral(notaFinal);

        var componentes = componentesTareas.Concat(componentesEvaluaciones).ToList();

        return new ConsolidatedGradeDto
        {
            AlumnoId = alumnoId,
            NombreAlumno = $"{alumno.Nombres} {alumno.Apellidos}",
            CursoId = cursoId,
            NombreCurso = "Curso", // Complementar con datos de repositorio
            PeriodoId = periodoId,
            NotaTareas = notaTareas,
            NotaEvaluaciones = notaEvaluaciones,
            NotaFinal = notaFinal,
            Literal = literal,
            Componentes = componentes,
            PesoPorcentajeTareas = pesoPorcentajeTareas,
            PesoPorcentajeEvaluaciones = pesoPorcentajeEvaluaciones
        };
    }

    /// <summary>
    /// Actualiza la nota de una entrega y registra auditoría.
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateGradeWithHistoryAsync(
        int entregaId,
        decimal nuevaNota,
        string razon,
        int docenteId)
    {
        var entrega = await _entregaTareaRepository.GetByIdAsync(entregaId)
            ?? throw new InvalidOperationException("Entrega no encontrada.");

        var notaAnterior = entrega.Puntos;

        // Registrar en auditoría
        await _auditTrailRepository.CreateAsync(new GradeAuditTrail
        {
            EntregaTareaId = entregaId,
            DocenteId = docenteId,
            NotaAnterior = notaAnterior,
            NotaNueva = nuevaNota,
            Razon = razon,
            Timestamp = DateTime.UtcNow
        });

        // Actualizar entrega
        entrega.Puntos = (int)nuevaNota;
        entrega.FechaCalificacion = DateTime.UtcNow;

        await _entregaTareaRepository.UpdateAsync(entrega);

        return (true, $"Calificación actualizada de {notaAnterior} a {nuevaNota}");
    }

    /// <summary>
    /// Obtiene estadísticas de una evaluación en un curso.
    /// </summary>
    public async Task<GradeStatisticsDto> GetGradeStatisticsAsync(int cursoId, int evaluacionId)
    {
        var evaluacion = await _evaluacionRepository.GetByIdAsync(evaluacionId)
            ?? throw new InvalidOperationException("Evaluación no encontrada.");

        var calificaciones = (await _calificacionRepository.GetByEvaluacionIdAsync(evaluacionId))
            .Where(c => c.EvaluacionId == evaluacionId)
            .ToList();

        if (calificaciones.Count == 0)
        {
            return new GradeStatisticsDto
            {
                CursoId = cursoId,
                EvaluacionId = evaluacionId,
                EvaluacionNombre = evaluacion.Nombre,
                Media = 0,
                Mediana = 0,
                DesviacionEstandar = 0,
                EstudiantesTotales = 0,
                AprobadosCount = 0,
                DesaprobadosCount = 0,
                PorcentajeAprobacion = 0
            };
        }

        var notas = calificaciones.Select(c => c.Nota).OrderBy(n => n).ToList();
        var media = notas.Average();
        var mediana = notas.Count % 2 == 0
            ? (notas[notas.Count / 2 - 1] + notas[notas.Count / 2]) / 2
            : notas[notas.Count / 2];

        var varianza = notas.Sum(n => Math.Pow((double)(n - (decimal)media), 2)) / notas.Count;
        var desviacionEstandar = (decimal)Math.Sqrt(varianza);

        var aprobados = notas.Count(n => n >= 60);
        var desaprobados = notas.Count - aprobados;

        return new GradeStatisticsDto
        {
            CursoId = cursoId,
            EvaluacionId = evaluacionId,
            EvaluacionNombre = evaluacion.Nombre,
            Media = media,
            Mediana = mediana,
            Minima = notas.Min(),
            Maxima = notas.Max(),
            DesviacionEstandar = desviacionEstandar,
            EstudiantesTotales = notas.Count,
            AprobadosCount = aprobados,
            DesaprobadosCount = desaprobados,
            PorcentajeAprobacion = (aprobados / (decimal)notas.Count) * 100m
        };
    }

    /// <summary>
    /// Calcula el literal de una nota (Excelente, Muy Bien, etc.).
    /// </summary>
    private static string CalculateGradeLiteral(decimal nota)
    {
        return nota switch
        {
            >= 90 => "Excelente",
            >= 80 => "Muy Bien",
            >= 70 => "Bien",
            >= 60 => "Satisfactorio",
            >= 50 => "Regular",
            _ => "Insuficiente"
        };
    }
}
