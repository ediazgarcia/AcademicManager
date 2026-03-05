namespace AcademicManager.Domain.Entities;

public class EntregaTarea
{
    public int Id { get; set; }
    public int TareaId { get; set; }
    public int AlumnoId { get; set; }
    public string? NombreArchivo { get; set; }
    public string? RutaArchivo { get; set; }
    public string? TipoArchivo { get; set; }
    public long? TamanoArchivo { get; set; }
    public string? Comentarios { get; set; }
    public DateTime FechaEntrega { get; set; } = DateTime.UtcNow;
    public bool EsTardia { get; set; } = false;
    public DateTime? FechaCalificacion { get; set; }
    public int? Puntos { get; set; }
    public string? Retroalimentacion { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation
    public Tarea? Tarea { get; set; }
    public Alumno? Alumno { get; set; }
}
