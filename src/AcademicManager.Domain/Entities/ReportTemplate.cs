namespace AcademicManager.Domain.Entities;

public class ReportTemplate
{
    public int Id { get; set; }
    public int CoordinadorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string TipoReporte { get; set; } = string.Empty; // "AprobacionState", "Regresion", "Coverage", etc.
    public string Filtros { get; set; } = string.Empty; // JSON serializado
    public bool Activa { get; set; } = true;
    public int Orden { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Usuario? Coordinador { get; set; }
}
