namespace AcademicManager.Domain.Entities;

public class StudentNotification
{
    public int Id { get; set; }
    public int AlumnoId { get; set; }
    public string Tipo { get; set; } = string.Empty; // "TaskDue", "GradePublished", "PlanificacionRejected", etc.
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public bool Leida { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LeidaAt { get; set; }

    // Navigation
    public Alumno? Alumno { get; set; }
}
