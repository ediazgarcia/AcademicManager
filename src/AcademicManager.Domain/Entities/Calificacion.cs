namespace AcademicManager.Domain.Entities;

public class Calificacion
{
    public int Id { get; set; }
    public int EvaluacionId { get; set; }
    public int AlumnoId { get; set; }
    public decimal Nota { get; set; } // The grade itself
    public decimal PuntosExtra { get; set; } // Extra points awarded or deducted
    public string Observacion { get; set; } = string.Empty; // Justification for grade/points
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navigation
    public Evaluacion? Evaluacion { get; set; }
    public Alumno? Alumno { get; set; }
}
