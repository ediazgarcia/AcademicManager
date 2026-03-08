namespace AcademicManager.Application.DTOs;

public sealed class WorkspaceMetric
{
    public string Label { get; init; } = string.Empty;
    public string Value { get; init; } = "0";
    public string Detail { get; init; } = string.Empty;
    public string Tone { get; init; } = "neutral";
    public string Icon { get; init; } = string.Empty;
}

public sealed class WorkspaceAction
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Href { get; init; } = "/";
    public string Icon { get; init; } = string.Empty;
}

public sealed class WorkspaceAlert
{
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Severity { get; init; } = "info";
    public string Href { get; init; } = "/";
    public string LinkLabel { get; init; } = "Abrir";
}

public sealed class TeacherPerformanceSummary
{
    public int DocenteId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string Especialidad { get; init; } = string.Empty;
    public int CursosAsignados { get; init; }
    public int EstudiantesAtendidos { get; init; }
    public int PlanesTotales { get; init; }
    public int PlanesAprobados { get; init; }
    public int PlanesPendientes { get; init; }
    public int PlanesEnBorrador { get; init; }
    public int Score { get; init; }
    public string NivelDesempeno { get; init; } = string.Empty;
    public string RiesgoOperativo { get; init; } = string.Empty;
    public string ProximaAccion { get; init; } = string.Empty;
    public DateTime? UltimaActividad { get; init; }
}

public sealed class StudentFollowUpSummary
{
    public int AlumnoId { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string GradoSeccion { get; init; } = string.Empty;
    public int CursosActivos { get; init; }
    public string Responsable { get; init; } = string.Empty;
    public string ContactoFamiliar { get; init; } = string.Empty;
    public string NivelSeguimiento { get; init; } = string.Empty;
    public string EstadoAcademico { get; init; } = string.Empty;
    public string Observacion { get; init; } = string.Empty;
}

public sealed class PlanningCourseSummary
{
    public int CursoId { get; init; }
    public int? PlanAnualId { get; init; }
    public string CodigoCurso { get; init; } = string.Empty;
    public string CursoNombre { get; init; } = string.Empty;
    public string DocenteLabel { get; init; } = string.Empty;
    public string SeccionLabel { get; init; } = string.Empty;
    public int EstudiantesImpactados { get; init; }
    public int PlanesBorrador { get; init; }
    public int PlanesEnviados { get; init; }
    public int PlanesAprobados { get; init; }
    public int PlanesRechazados { get; init; }
    public int Cobertura { get; init; }
    public string EstadoPrincipal { get; init; } = string.Empty;
    public string ProximoPaso { get; init; } = string.Empty;
    public DateTime? UltimaActualizacion { get; init; }
}

public sealed class AcademicWorkspaceSnapshot
{
    public string Rol { get; init; } = string.Empty;
    public string PeriodoActivo { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public IReadOnlyList<WorkspaceMetric> Metrics { get; init; } = [];
    public IReadOnlyList<WorkspaceAction> Actions { get; init; } = [];
    public IReadOnlyList<TeacherPerformanceSummary> Teachers { get; init; } = [];
    public IReadOnlyList<StudentFollowUpSummary> Students { get; init; } = [];
    public IReadOnlyList<PlanningCourseSummary> Planning { get; init; } = [];
    public IReadOnlyList<WorkspaceAlert> Alerts { get; init; } = [];
}
