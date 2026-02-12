namespace AcademicManager.Domain.Entities;

public class PeriodoAcademico
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;         // "2026-I", "2026-II", "Año 2026"
    public string Tipo { get; set; } = string.Empty;           // Semestre, Año
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
