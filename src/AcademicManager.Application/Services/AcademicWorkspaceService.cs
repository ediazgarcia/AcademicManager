using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class AcademicWorkspaceService
{
    private const string AdminRole = "Admin";
    private const string CoordinadorRole = "Coordinador";
    private const string DocenteRole = "Docente";
    private const string AlumnoRole = "Alumno";

    private readonly IAlumnoRepository _alumnoRepository;
    private readonly ICursoRepository _cursoRepository;
    private readonly IDocenteCursoRepository _docenteCursoRepository;
    private readonly IDocenteRepository _docenteRepository;
    private readonly IGradoRepository _gradoRepository;
    private readonly IMatriculaCursoRepository _matriculaCursoRepository;
    private readonly IPeriodoAcademicoRepository _periodoAcademicoRepository;
    private readonly IPlanificacionRepository _planificacionRepository;
    private readonly ISeccionRepository _seccionRepository;

    public AcademicWorkspaceService(
        IAlumnoRepository alumnoRepository,
        ICursoRepository cursoRepository,
        IDocenteCursoRepository docenteCursoRepository,
        IDocenteRepository docenteRepository,
        IGradoRepository gradoRepository,
        IMatriculaCursoRepository matriculaCursoRepository,
        IPeriodoAcademicoRepository periodoAcademicoRepository,
        IPlanificacionRepository planificacionRepository,
        ISeccionRepository seccionRepository)
    {
        _alumnoRepository = alumnoRepository;
        _cursoRepository = cursoRepository;
        _docenteCursoRepository = docenteCursoRepository;
        _docenteRepository = docenteRepository;
        _gradoRepository = gradoRepository;
        _matriculaCursoRepository = matriculaCursoRepository;
        _periodoAcademicoRepository = periodoAcademicoRepository;
        _planificacionRepository = planificacionRepository;
        _seccionRepository = seccionRepository;
    }

    public async Task<AcademicWorkspaceSnapshot> ObtenerPanelPrincipalAsync(string? rol, int? docenteId, int? alumnoId)
    {
        var normalizedRole = NormalizeRole(rol);
        var context = await LoadContextAsync();
        var docentes = BuildTeacherSummaries(context);

        if (IsTeacherRole(normalizedRole))
        {
            var docentePanel = docentes.FirstOrDefault(item => item.DocenteId == docenteId);
            var students = BuildStudentSummaries(context, docenteId).Take(8).ToList();
            var planning = BuildPlanningSummaries(context, normalizedRole, docenteId, null).Take(6).ToList();
            var coverage = planning.Any() ? (int)Math.Round(planning.Average(item => item.Cobertura)) : 0;

            return new AcademicWorkspaceSnapshot
            {
                Rol = normalizedRole,
                PeriodoActivo = context.PeriodoActivo,
                Title = docentePanel is null
                    ? "Mesa docente"
                    : $"Mesa docente de {ExtractFirstName(docentePanel.NombreCompleto)}",
                Subtitle = "Planifica, monitorea estudiantes y mantén el ritmo de tu seccion desde un solo workspace.",
                Metrics =
                [
                    CreateMetric("Cursos asignados", docentePanel?.CursosAsignados ?? 0, "Carga academica activa", "primary", "PI"),
                    CreateMetric("Estudiantes", docentePanel?.EstudiantesAtendidos ?? 0, "Bajo tu seguimiento", "success", "ES"),
                    CreateMetric("Cobertura", $"{coverage}%", "Avance de planificacion", "warning", "CO"),
                    CreateMetric("Atencion prioritaria", students.Count(item => !IsStableStudent(item)), "Casos a acompanar esta semana", "danger", "AT")
                ],
                Actions =
                [
                    CreateAction("Planificar clase", "Abre tu flujo anual, mensual y diario.", "/planificador", "PL"),
                    CreateAction("Seguimiento estudiantil", "Consulta tu cartera de estudiantes.", "/alumnos", "AL"),
                    CreateAction("Ver horario", "Organiza tu jornada por bloque.", "/horarios", "HO"),
                    CreateAction("Gestionar tareas", "Mantente sobre entregas y publicaciones.", "/tareas", "TA")
                ],
                Teachers = docentePanel is null ? [] : [docentePanel],
                Students = students,
                Planning = planning,
                Alerts = BuildTeacherAlerts(docentePanel, students, planning)
            };
        }

        if (IsStudentRole(normalizedRole))
        {
            var students = BuildStudentSummaries(context)
                .Where(item => item.AlumnoId == alumnoId)
                .Take(1)
                .ToList();
            var planning = BuildPlanningSummaries(context, normalizedRole, null, alumnoId).Take(6).ToList();
            var summary = students.FirstOrDefault();
            var docentesRelacionados = summary is null || string.IsNullOrWhiteSpace(summary.Responsable)
                ? 0
                : summary.Responsable.Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
            var alerts = BuildStudentAlerts(summary, planning);
            var coverage = planning.Any() ? (int)Math.Round(planning.Average(item => item.Cobertura)) : 0;

            return new AcademicWorkspaceSnapshot
            {
                Rol = normalizedRole,
                PeriodoActivo = context.PeriodoActivo,
                Title = summary is null
                    ? "Mi espacio academico"
                    : $"Aprendizaje de {ExtractFirstName(summary.NombreCompleto)}",
                Subtitle = "Consulta tus cursos, docentes responsables y el estado de la planificacion que sostiene tus clases.",
                Metrics =
                [
                    CreateMetric("Cursos activos", summary?.CursosActivos ?? 0, "Materias en tu carga", "primary", "CU"),
                    CreateMetric("Docentes", docentesRelacionados, "Responsables vinculados", "success", "DO"),
                    CreateMetric("Cobertura", $"{coverage}%", "Planificacion disponible", "warning", "PL"),
                    CreateMetric("Alertas", alerts.Count, "Asuntos por resolver", "danger", "AL")
                ],
                Actions =
                [
                    CreateAction("Mis tareas", "Revisa tus entregables publicados.", "/mis-tareas", "MT"),
                    CreateAction("Cursos", "Consulta la oferta academica que cursas.", "/cursos", "CU"),
                    CreateAction("Horario", "Ubica tus bloques del dia.", "/horarios", "HO")
                ],
                Students = students,
                Planning = planning,
                Alerts = alerts
            };
        }

        var teacherList = docentes.Take(6).ToList();
        var studentList = BuildStudentSummaries(context).Take(8).ToList();
        var planningList = BuildPlanningSummaries(context, normalizedRole, null, null).Take(6).ToList();
        var pendingReviews = planningList.Sum(item => item.PlanesEnviados);
        var institutionalCoverage = planningList.Any()
            ? (int)Math.Round(planningList.Average(item => item.Cobertura))
            : 0;

        return new AcademicWorkspaceSnapshot
        {
            Rol = normalizedRole,
            PeriodoActivo = context.PeriodoActivo,
            Title = IsCoordinatorRole(normalizedRole)
                ? "Centro de coordinacion academica"
                : "Panel institucional",
            Subtitle = "Supervisa el trabajo docente, detecta desbalances y mantén trazabilidad completa sobre la experiencia del estudiante.",
            Metrics =
            [
                CreateMetric("Docentes activos", context.Docentes.Count(item => item.Activo), "Equipo docente habilitado", "primary", "DO"),
                CreateMetric("Planes por revisar", pendingReviews, "Enviados por docentes", "warning", "RV"),
                CreateMetric("Seguimiento prioritario", studentList.Count(item => !IsStableStudent(item)), "Estudiantes que requieren accion", "danger", "SE"),
                CreateMetric("Cobertura institucional", $"{institutionalCoverage}%", "Salud del ciclo de planificacion", "success", "CI")
            ],
            Actions =
            [
                CreateAction("Supervisar docentes", "Abre el tablero de desempeno y carga.", "/docentes", "DO"),
                CreateAction("Monitorear estudiantes", "Gestiona acompanamiento y contacto familiar.", "/alumnos", "AL"),
                CreateAction("Revisar planificacion", "Controla aprobaciones y huecos operativos.", "/planificador", "PL"),
                CreateAction("Usuarios y accesos", "Configura coordinadores, admins y solicitudes.", "/usuarios", "US")
            ],
            Teachers = teacherList,
            Students = studentList,
            Planning = planningList,
            Alerts = BuildCoordinatorAlerts(teacherList, studentList, planningList)
        };
    }

    public async Task<IReadOnlyList<TeacherPerformanceSummary>> ObtenerResumenDocentesAsync()
    {
        var context = await LoadContextAsync();
        return BuildTeacherSummaries(context);
    }

    public async Task<IReadOnlyList<StudentFollowUpSummary>> ObtenerResumenAlumnosAsync(int? docenteId = null)
    {
        var context = await LoadContextAsync();
        return BuildStudentSummaries(context, docenteId);
    }

    public async Task<IReadOnlyList<PlanningCourseSummary>> ObtenerResumenPlanificacionAsync(string? rol, int? docenteId = null, int? alumnoId = null)
    {
        var context = await LoadContextAsync();
        return BuildPlanningSummaries(context, NormalizeRole(rol), docenteId, alumnoId);
    }

    private async Task<WorkspaceContext> LoadContextAsync()
    {
        var docentesTask = _docenteRepository.GetAllAsync();
        var alumnosTask = _alumnoRepository.GetAllAsync();
        var cursosTask = _cursoRepository.GetAllAsync();
        var gradosTask = _gradoRepository.GetAllAsync();
        var seccionesTask = _seccionRepository.GetAllAsync();
        var planesTask = _planificacionRepository.GetAllAsync();
        var periodoTask = _periodoAcademicoRepository.GetActivoAsync();

        await Task.WhenAll(docentesTask, alumnosTask, cursosTask, gradosTask, seccionesTask, planesTask, periodoTask);

        var docentes = (await docentesTask).OrderBy(item => item.Apellidos).ThenBy(item => item.Nombres).ToList();
        var alumnos = (await alumnosTask).OrderBy(item => item.Apellidos).ThenBy(item => item.Nombres).ToList();
        var cursos = (await cursosTask).Where(item => item.Activo).OrderBy(item => item.Nombre).ToList();
        var grados = (await gradosTask).ToList();
        var secciones = (await seccionesTask).ToList();
        var planes = (await planesTask).ToList();
        var periodoActivo = (await periodoTask)?.Nombre ?? "Sin periodo activo";

        var docenteCursoTasks = docentes
            .Select(async item => new IdMap(item.Id,
                (await _docenteCursoRepository.GetByDocenteIdAsync(item.Id))
                    .Where(link => link.Activo)
                    .Select(link => link.CursoId)
                    .Distinct()
                    .ToList()))
            .ToArray();

        var matriculaTasks = cursos
            .Select(async item => new IdMap(item.Id,
                (await _matriculaCursoRepository.GetByCursoIdAsync(item.Id))
                    .Where(link => link.Activo)
                    .Select(link => link.AlumnoId)
                    .Distinct()
                    .ToList()))
            .ToArray();

        await Task.WhenAll(docenteCursoTasks.Cast<Task>().Concat(matriculaTasks.Cast<Task>()));

        var courseIdsByDocente = docenteCursoTasks
            .Select(task => task.Result)
            .ToDictionary(item => item.Id, item => (IReadOnlyList<int>)item.Items);

        var studentIdsByCourse = matriculaTasks
            .Select(task => task.Result)
            .ToDictionary(item => item.Id, item => (IReadOnlyList<int>)item.Items);

        var courseIdsByAlumno = studentIdsByCourse
            .SelectMany(
                item => item.Value.Select(alumnoId => new { AlumnoId = alumnoId, CursoId = item.Key }))
            .GroupBy(item => item.AlumnoId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)group
                    .Select(item => item.CursoId)
                    .Distinct()
                    .OrderBy(item => item)
                    .ToList());

        var docenteIdsByCourse = courseIdsByDocente
            .SelectMany(
                item => item.Value.Select(cursoId => new { CursoId = cursoId, DocenteId = item.Key }))
            .GroupBy(item => item.CursoId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<int>)group
                    .Select(item => item.DocenteId)
                    .Distinct()
                    .OrderBy(item => item)
                    .ToList());

        return new WorkspaceContext
        {
            PeriodoActivo = periodoActivo,
            Docentes = docentes,
            Alumnos = alumnos,
            Cursos = cursos,
            Grados = grados,
            Secciones = secciones,
            Planes = planes,
            CourseIdsByDocente = courseIdsByDocente,
            StudentIdsByCourse = studentIdsByCourse,
            CourseIdsByAlumno = courseIdsByAlumno,
            DocenteIdsByCourse = docenteIdsByCourse
        };
    }

    private static List<TeacherPerformanceSummary> BuildTeacherSummaries(WorkspaceContext context)
    {
        return context.Docentes
            .Select(docente =>
            {
                var courseIds = GetOrEmpty(context.CourseIdsByDocente, docente.Id);
                var studentCount = courseIds
                    .SelectMany(courseId => GetOrEmpty(context.StudentIdsByCourse, courseId))
                    .Distinct()
                    .Count();
                var docPlans = context.Planes.Where(plan => plan.DocenteId == docente.Id).ToList();
                var approved = docPlans.Count(plan => IsApprovedState(plan.Estado));
                var pending = docPlans.Count(plan => IsSentState(plan.Estado));
                var rejected = docPlans.Count(plan => IsRejectedState(plan.Estado));
                var draft = docPlans.Count(plan => IsDraftState(plan.Estado));
                var score = CalculateTeacherScore(courseIds.Count, studentCount, approved, pending, draft, rejected);

                return new TeacherPerformanceSummary
                {
                    DocenteId = docente.Id,
                    Codigo = docente.Codigo,
                    NombreCompleto = BuildFullName(docente.Nombres, docente.Apellidos),
                    Especialidad = string.IsNullOrWhiteSpace(docente.Especialidad) ? "Especialidad por definir" : docente.Especialidad,
                    CursosAsignados = courseIds.Count,
                    EstudiantesAtendidos = studentCount,
                    PlanesTotales = docPlans.Count,
                    PlanesAprobados = approved,
                    PlanesPendientes = pending,
                    PlanesEnBorrador = draft,
                    Score = score,
                    NivelDesempeno = ResolveTeacherLevel(score),
                    RiesgoOperativo = ResolveTeacherRisk(courseIds.Count, pending, rejected, draft),
                    ProximaAccion = ResolveTeacherNextAction(courseIds.Count, docPlans.Count, pending, rejected),
                    UltimaActividad = docPlans
                        .Select(plan => plan.FechaActualizacion ?? plan.FechaCreacion)
                        .OrderByDescending(date => date)
                        .FirstOrDefault()
                };
            })
            .OrderBy(item => item.Score)
            .ThenByDescending(item => item.PlanesPendientes)
            .ThenBy(item => item.NombreCompleto)
            .ToList();
    }

    private static List<StudentFollowUpSummary> BuildStudentSummaries(WorkspaceContext context, int? docenteId = null)
    {
        var allowedCourseIds = docenteId.HasValue
            ? GetOrEmpty(context.CourseIdsByDocente, docenteId.Value).ToHashSet()
            : null;

        var allowedStudentIds = allowedCourseIds is null
            ? null
            : context.StudentIdsByCourse
                .Where(item => allowedCourseIds.Contains(item.Key))
                .SelectMany(item => item.Value)
                .Distinct()
                .ToHashSet();

        var gradesById = context.Grados.ToDictionary(item => item.Id);
        var sectionsById = context.Secciones.ToDictionary(item => item.Id);
        var docentesById = context.Docentes.ToDictionary(item => item.Id);

        return context.Alumnos
            .Where(item => allowedStudentIds is null || allowedStudentIds.Contains(item.Id))
            .Select(alumno =>
            {
                var courseIds = GetOrEmpty(context.CourseIdsByAlumno, alumno.Id);
                var visibleCourseIds = allowedCourseIds is null
                    ? courseIds
                    : courseIds.Where(courseId => allowedCourseIds.Contains(courseId)).ToList();
                var responsableIds = visibleCourseIds
                    .SelectMany(courseId => GetOrEmpty(context.DocenteIdsByCourse, courseId))
                    .Distinct()
                    .ToList();

                var responsables = responsableIds
                    .Select(id => docentesById.GetValueOrDefault(id))
                    .Where(docente => docente is not null)
                    .Select(docente => BuildCompactTeacherName(docente!))
                    .Distinct()
                    .ToList();

                var gradoNombre = alumno.GradoId.HasValue && gradesById.TryGetValue(alumno.GradoId.Value, out var grado)
                    ? grado.Nombre
                    : "Sin grado";
                var seccionNombre = alumno.SeccionId.HasValue && sectionsById.TryGetValue(alumno.SeccionId.Value, out var seccion)
                    ? seccion.Nombre
                    : "Sin seccion";
                var activeCourses = visibleCourseIds.Count;
                var estadoAcademico = ResolveStudentState(alumno.Activo, activeCourses);
                var nivelSeguimiento = ResolveStudentTrackingLevel(alumno, activeCourses);

                return new StudentFollowUpSummary
                {
                    AlumnoId = alumno.Id,
                    Codigo = alumno.Codigo,
                    NombreCompleto = BuildFullName(alumno.Nombres, alumno.Apellidos),
                    GradoSeccion = $"{gradoNombre} · {seccionNombre}",
                    CursosActivos = activeCourses,
                    Responsable = responsables.Count > 0 ? string.Join(", ", responsables) : "Sin docente asignado",
                    ContactoFamiliar = BuildFamilyContact(alumno),
                    NivelSeguimiento = nivelSeguimiento,
                    EstadoAcademico = estadoAcademico,
                    Observacion = BuildStudentObservation(alumno, activeCourses, responsables.Count)
                };
            })
            .OrderBy(item => GetStudentPriority(item.NivelSeguimiento))
            .ThenBy(item => item.NombreCompleto)
            .ToList();
    }

    private static List<PlanningCourseSummary> BuildPlanningSummaries(
        WorkspaceContext context,
        string normalizedRole,
        int? docenteId,
        int? alumnoId)
    {
        IReadOnlyList<int> courseIds;

        if (IsStudentRole(normalizedRole) && alumnoId.HasValue)
        {
            courseIds = GetOrEmpty(context.CourseIdsByAlumno, alumnoId.Value);
        }
        else if (IsTeacherRole(normalizedRole) && docenteId.HasValue)
        {
            courseIds = GetOrEmpty(context.CourseIdsByDocente, docenteId.Value);
        }
        else
        {
            courseIds = context.Cursos.Select(item => item.Id).ToList();
        }

        var coursesById = context.Cursos.ToDictionary(item => item.Id);
        var docentesById = context.Docentes.ToDictionary(item => item.Id);

        return courseIds
            .Distinct()
            .Select(courseId =>
            {
                if (!coursesById.TryGetValue(courseId, out var curso))
                {
                    return null;
                }

                var plans = context.Planes
                    .Where(plan => plan.CursoId == courseId)
                    .Where(plan => !docenteId.HasValue || !IsTeacherRole(normalizedRole) || plan.DocenteId == docenteId.Value)
                    .ToList();

                var approved = plans.Count(plan => IsApprovedState(plan.Estado));
                var sent = plans.Count(plan => IsSentState(plan.Estado));
                var rejected = plans.Count(plan => IsRejectedState(plan.Estado));
                var draft = plans.Count(plan => IsDraftState(plan.Estado));
                var teacherNames = GetOrEmpty(context.DocenteIdsByCourse, courseId)
                    .Select(id => docentesById.GetValueOrDefault(id))
                    .Where(docente => docente is not null)
                    .Select(docente => BuildCompactTeacherName(docente!))
                    .Distinct()
                    .ToList();

                var sectionNames = plans
                    .Select(plan => plan.Seccion?.Nombre)
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Cast<string>()
                    .Distinct()
                    .ToList();

                return new PlanningCourseSummary
                {
                    CursoId = curso.Id,
                    PlanAnualId = plans
                        .OrderByDescending(plan => plan.FechaActualizacion ?? plan.FechaCreacion)
                        .Select(plan => (int?)plan.Id)
                        .FirstOrDefault(),
                    CodigoCurso = curso.Codigo,
                    CursoNombre = curso.Nombre,
                    DocenteLabel = teacherNames.Count > 0 ? string.Join(", ", teacherNames) : "Sin docente asignado",
                    SeccionLabel = sectionNames.Count > 0 ? string.Join(", ", sectionNames) : "Sin seccion definida",
                    EstudiantesImpactados = GetOrEmpty(context.StudentIdsByCourse, courseId).Distinct().Count(),
                    PlanesBorrador = draft,
                    PlanesEnviados = sent,
                    PlanesAprobados = approved,
                    PlanesRechazados = rejected,
                    Cobertura = CalculatePlanningCoverage(plans),
                    EstadoPrincipal = ResolvePlanningStatus(approved, sent, rejected, draft),
                    ProximoPaso = ResolvePlanningNextAction(approved, sent, rejected, draft),
                    UltimaActualizacion = plans
                        .Select(plan => plan.FechaActualizacion ?? plan.FechaCreacion)
                        .OrderByDescending(date => date)
                        .FirstOrDefault()
                };
            })
            .Where(item => item is not null)
            .Cast<PlanningCourseSummary>()
            .OrderBy(item => item.Cobertura)
            .ThenByDescending(item => item.PlanesEnviados)
            .ThenBy(item => item.CursoNombre)
            .ToList();
    }

    private static List<WorkspaceAlert> BuildCoordinatorAlerts(
        IReadOnlyList<TeacherPerformanceSummary> teachers,
        IReadOnlyList<StudentFollowUpSummary> students,
        IReadOnlyList<PlanningCourseSummary> planning)
    {
        var alerts = new List<WorkspaceAlert>();

        var teacherRisk = teachers.FirstOrDefault(item => item.Score < 60);
        if (teacherRisk is not null)
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = $"Acompanamiento docente: {teacherRisk.NombreCompleto}",
                Description = $"{teacherRisk.RiesgoOperativo}. Proximo paso sugerido: {teacherRisk.ProximaAccion}.",
                Severity = "danger",
                Href = "/docentes",
                LinkLabel = "Revisar docente"
            });
        }

        var sentPlans = planning.Sum(item => item.PlanesEnviados);
        if (sentPlans > 0)
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Revisiones pendientes en planificacion",
                Description = $"Hay {sentPlans} plan(es) enviados esperando validacion del coordinador.",
                Severity = "warning",
                Href = "/planificador",
                LinkLabel = "Abrir flujo"
            });
        }

        var studentAttention = students.Count(item => !IsStableStudent(item));
        if (studentAttention > 0)
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Estudiantes con seguimiento activo",
                Description = $"{studentAttention} estudiante(s) tienen carga parcial, falta de contacto o requieren soporte inmediato.",
                Severity = "info",
                Href = "/alumnos",
                LinkLabel = "Ver estudiantes"
            });
        }

        if (!alerts.Any())
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Operacion estable",
                Description = "No se detectaron alertas criticas en la vista institucional.",
                Severity = "success",
                Href = "/",
                LinkLabel = "Actualizar"
            });
        }

        return alerts;
    }

    private static List<WorkspaceAlert> BuildTeacherAlerts(
        TeacherPerformanceSummary? teacher,
        IReadOnlyList<StudentFollowUpSummary> students,
        IReadOnlyList<PlanningCourseSummary> planning)
    {
        var alerts = new List<WorkspaceAlert>();

        if (teacher is null || teacher.CursosAsignados == 0)
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Sin carga docente asignada",
                Description = "Solicita a coordinacion la asignacion de cursos para iniciar tu planificacion.",
                Severity = "warning",
                Href = "/docentes",
                LinkLabel = "Ir a coordinacion"
            });
        }

        if (planning.Any(item => item.Cobertura == 0))
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Cursos sin planificacion activa",
                Description = "Existe al menos un curso sin plan anual levantado.",
                Severity = "danger",
                Href = "/planificador",
                LinkLabel = "Planificar"
            });
        }

        var priorityStudents = students.Count(item => !IsStableStudent(item));
        if (priorityStudents > 0)
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Seguimiento estudiantil prioritario",
                Description = $"{priorityStudents} estudiante(s) requieren contacto, carga academica o acompanamiento esta semana.",
                Severity = "info",
                Href = "/alumnos",
                LinkLabel = "Abrir cartera"
            });
        }

        if (!alerts.Any())
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Semana controlada",
                Description = "Tu carga actual no presenta bloqueos operativos.",
                Severity = "success",
                Href = "/planificador",
                LinkLabel = "Seguir trabajando"
            });
        }

        return alerts;
    }

    private static List<WorkspaceAlert> BuildStudentAlerts(
        StudentFollowUpSummary? student,
        IReadOnlyList<PlanningCourseSummary> planning)
    {
        var alerts = new List<WorkspaceAlert>();

        if (student is null)
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Perfil estudiantil pendiente",
                Description = "Tu cuenta aun no esta vinculada con un registro academico completo.",
                Severity = "warning",
                Href = "/perfil",
                LinkLabel = "Ver perfil"
            });

            return alerts;
        }

        if (student.CursosActivos == 0)
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Sin cursos matriculados",
                Description = "Tu carga academica aun no ha sido activada en el sistema.",
                Severity = "danger",
                Href = "/cursos",
                LinkLabel = "Ver cursos"
            });
        }

        if (planning.Any(item => item.Cobertura < 40))
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Cobertura de planificacion incompleta",
                Description = "Algunos cursos aun estan en borrador o sin plan visible.",
                Severity = "warning",
                Href = "/planificador",
                LinkLabel = "Consultar estado"
            });
        }

        if (!alerts.Any())
        {
            alerts.Add(new WorkspaceAlert
            {
                Title = "Trayectoria academica visible",
                Description = "Tus cursos cuentan con acompanamiento y estructura activa.",
                Severity = "success",
                Href = "/mis-tareas",
                LinkLabel = "Ir a tareas"
            });
        }

        return alerts;
    }

    private static WorkspaceMetric CreateMetric(string label, int value, string detail, string tone, string icon) =>
        CreateMetric(label, value.ToString(), detail, tone, icon);

    private static WorkspaceMetric CreateMetric(string label, string value, string detail, string tone, string icon) =>
        new()
        {
            Label = label,
            Value = value,
            Detail = detail,
            Tone = tone,
            Icon = icon
        };

    private static WorkspaceAction CreateAction(string title, string description, string href, string icon) =>
        new()
        {
            Title = title,
            Description = description,
            Href = href,
            Icon = icon
        };

    private static IReadOnlyList<int> GetOrEmpty(IReadOnlyDictionary<int, IReadOnlyList<int>> source, int key) =>
        source.TryGetValue(key, out var values) ? values : [];

    private static string NormalizeRole(string? rol)
    {
        if (string.Equals(rol, CoordinadorRole, StringComparison.OrdinalIgnoreCase))
        {
            return CoordinadorRole;
        }

        if (string.Equals(rol, DocenteRole, StringComparison.OrdinalIgnoreCase))
        {
            return DocenteRole;
        }

        if (string.Equals(rol, AlumnoRole, StringComparison.OrdinalIgnoreCase))
        {
            return AlumnoRole;
        }

        return AdminRole;
    }

    private static bool IsCoordinatorRole(string rol) =>
        string.Equals(rol, AdminRole, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(rol, CoordinadorRole, StringComparison.OrdinalIgnoreCase);

    private static bool IsTeacherRole(string rol) =>
        string.Equals(rol, DocenteRole, StringComparison.OrdinalIgnoreCase);

    private static bool IsStudentRole(string rol) =>
        string.Equals(rol, AlumnoRole, StringComparison.OrdinalIgnoreCase);

    private static bool IsApprovedState(string? estado) =>
        string.Equals(estado, "Aprobado", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(estado, "Evaluado", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(estado, "Ejecutandose", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(estado, "Ejecutándose", StringComparison.OrdinalIgnoreCase);

    private static bool IsSentState(string? estado) =>
        string.Equals(estado, "Enviado", StringComparison.OrdinalIgnoreCase);

    private static bool IsRejectedState(string? estado) =>
        string.Equals(estado, "Rechazado", StringComparison.OrdinalIgnoreCase);

    private static bool IsDraftState(string? estado) =>
        string.IsNullOrWhiteSpace(estado) ||
        string.Equals(estado, "Borrador", StringComparison.OrdinalIgnoreCase);

    private static int CalculateTeacherScore(int courseCount, int studentCount, int approved, int pending, int draft, int rejected)
    {
        var score = 35;

        score += courseCount > 0 ? 15 : -10;
        score += studentCount > 0 ? 10 : 0;
        score += approved * 10;
        score += draft > 0 ? 5 : 0;
        score += pending == 0 ? 10 : Math.Max(2, 10 - (pending * 3));
        score -= rejected * 8;

        return Math.Clamp(score, 0, 100);
    }

    private static string ResolveTeacherLevel(int score) => score switch
    {
        >= 85 => "Sobresaliente",
        >= 70 => "Consistente",
        >= 55 => "En seguimiento",
        _ => "Prioritario"
    };

    private static string ResolveTeacherRisk(int courseCount, int pending, int rejected, int draft)
    {
        if (courseCount == 0)
        {
            return "Sin carga academica";
        }

        if (pending > 0)
        {
            return "Tiene revision pendiente";
        }

        if (rejected > 0)
        {
            return "Requiere ajustes de plan";
        }

        if (draft > 0)
        {
            return "Planificacion en construccion";
        }

        return "Operacion controlada";
    }

    private static string ResolveTeacherNextAction(int courseCount, int planCount, int pending, int rejected)
    {
        if (courseCount == 0)
        {
            return "Solicitar asignacion de cursos";
        }

        if (planCount == 0)
        {
            return "Crear plan anual";
        }

        if (pending > 0)
        {
            return "Dar seguimiento a planes enviados";
        }

        if (rejected > 0)
        {
            return "Corregir observaciones del coordinador";
        }

        return "Mantener acompanamiento sobre estudiantes";
    }

    private static string ResolveStudentState(bool activo, int activeCourses)
    {
        if (!activo)
        {
            return "Inactivo";
        }

        if (activeCourses == 0)
        {
            return "Sin matricula";
        }

        if (activeCourses == 1)
        {
            return "Carga parcial";
        }

        return "Carga activa";
    }

    private static string ResolveStudentTrackingLevel(Alumno alumno, int activeCourses)
    {
        if (!alumno.Activo || activeCourses == 0)
        {
            return "Prioritario";
        }

        if (string.IsNullOrWhiteSpace(alumno.NombreApoderado) || string.IsNullOrWhiteSpace(alumno.TelefonoApoderado))
        {
            return "Atencion";
        }

        if (activeCourses == 1)
        {
            return "En seguimiento";
        }

        return "Estable";
    }

    private static string BuildStudentObservation(Alumno alumno, int activeCourses, int responsableCount)
    {
        if (!alumno.Activo)
        {
            return "Registro inactivo. Revisar continuidad.";
        }

        if (activeCourses == 0)
        {
            return "Sin matricula. Validar asignacion academica.";
        }

        if (responsableCount == 0)
        {
            return "Sin docente responsable registrado.";
        }

        if (string.IsNullOrWhiteSpace(alumno.TelefonoApoderado))
        {
            return "Completar contacto familiar.";
        }

        return "Seguimiento ordinario en curso.";
    }

    private static string BuildFamilyContact(Alumno alumno)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(alumno.NombreApoderado))
        {
            parts.Add(alumno.NombreApoderado);
        }

        if (!string.IsNullOrWhiteSpace(alumno.TelefonoApoderado))
        {
            parts.Add(alumno.TelefonoApoderado);
        }

        return parts.Count > 0 ? string.Join(" · ", parts) : "Sin contacto registrado";
    }

    private static int GetStudentPriority(string level) => level switch
    {
        "Prioritario" => 0,
        "Atencion" => 1,
        "En seguimiento" => 2,
        _ => 3
    };

    private static bool IsStableStudent(StudentFollowUpSummary summary) =>
        string.Equals(summary.NivelSeguimiento, "Estable", StringComparison.OrdinalIgnoreCase);

    private static int CalculatePlanningCoverage(IReadOnlyCollection<Planificacion> plans)
    {
        if (plans.Count == 0)
        {
            return 0;
        }

        var weightedScore = plans.Sum(plan =>
        {
            if (IsApprovedState(plan.Estado))
            {
                return 100;
            }

            if (IsSentState(plan.Estado))
            {
                return 70;
            }

            if (IsRejectedState(plan.Estado))
            {
                return 25;
            }

            return 35;
        });

        return (int)Math.Round(weightedScore / (double)plans.Count);
    }

    private static string ResolvePlanningStatus(int approved, int sent, int rejected, int draft)
    {
        if (approved > 0)
        {
            return "Aprobado";
        }

        if (sent > 0)
        {
            return "Por revisar";
        }

        if (rejected > 0)
        {
            return "Requiere ajustes";
        }

        if (draft > 0)
        {
            return "En borrador";
        }

        return "Sin plan";
    }

    private static string ResolvePlanningNextAction(int approved, int sent, int rejected, int draft)
    {
        if (approved > 0)
        {
            return "Dar seguimiento a ejecucion y evidencias";
        }

        if (sent > 0)
        {
            return "Esperar revision de coordinacion";
        }

        if (rejected > 0)
        {
            return "Ajustar observaciones y reenviar";
        }

        if (draft > 0)
        {
            return "Completar y enviar plan";
        }

        return "Crear plan anual para este curso";
    }

    private static string BuildFullName(string nombres, string apellidos)
    {
        var parts = new[] { nombres?.Trim(), apellidos?.Trim() }
            .Where(value => !string.IsNullOrWhiteSpace(value));

        return string.Join(' ', parts);
    }

    private static string BuildCompactTeacherName(Docente docente)
    {
        if (!string.IsNullOrWhiteSpace(docente.Apellidos))
        {
            return docente.Apellidos;
        }

        return ExtractFirstName(BuildFullName(docente.Nombres, docente.Apellidos));
    }

    private static string ExtractFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return "Equipo";
        }

        return fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? fullName;
    }

    private sealed class WorkspaceContext
    {
        public string PeriodoActivo { get; init; } = string.Empty;
        public IReadOnlyList<Docente> Docentes { get; init; } = [];
        public IReadOnlyList<Alumno> Alumnos { get; init; } = [];
        public IReadOnlyList<Curso> Cursos { get; init; } = [];
        public IReadOnlyList<Grado> Grados { get; init; } = [];
        public IReadOnlyList<Seccion> Secciones { get; init; } = [];
        public IReadOnlyList<Planificacion> Planes { get; init; } = [];
        public IReadOnlyDictionary<int, IReadOnlyList<int>> CourseIdsByDocente { get; init; } =
            new Dictionary<int, IReadOnlyList<int>>();
        public IReadOnlyDictionary<int, IReadOnlyList<int>> StudentIdsByCourse { get; init; } =
            new Dictionary<int, IReadOnlyList<int>>();
        public IReadOnlyDictionary<int, IReadOnlyList<int>> CourseIdsByAlumno { get; init; } =
            new Dictionary<int, IReadOnlyList<int>>();
        public IReadOnlyDictionary<int, IReadOnlyList<int>> DocenteIdsByCourse { get; init; } =
            new Dictionary<int, IReadOnlyList<int>>();
    }

    private sealed record IdMap(int Id, IReadOnlyList<int> Items);
}
