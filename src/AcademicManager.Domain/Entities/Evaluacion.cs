namespace AcademicManager.Domain.Entities;

public class Evaluacion
{
    public int Id { get; set; }
    public int PlanificacionId { get; set; }
    public string Nombre { get; set; } = string.Empty; // "Parcial 1", "Tarea 1"
    public string Descripcion { get; set; } = string.Empty;
    public decimal Peso { get; set; } // Percentage (0-100) or Points
    public decimal MaximoPuntaje { get; set; } = 20m; // Default to 20 or 100
    public DateTime FechaEvaluacion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navigation
    public Planificacion? Planificacion { get; set; }
}
