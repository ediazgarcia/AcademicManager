namespace AcademicManager.Domain.Entities;

public class Asistencia
{
    public int Id { get; set; }
    public int PlanificacionId { get; set; } // Links to the course/class
    public int AlumnoId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Today;
    public string Estado { get; set; } = "Presente"; // Presente, Ausente, Tarde, Justificado
    public string Observacion { get; set; } = string.Empty; // Justification for absence/late
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navigation
    public Planificacion? Planificacion { get; set; }
    public Alumno? Alumno { get; set; }
}
