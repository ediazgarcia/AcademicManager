namespace AcademicManager.Domain.Entities;

public class GradeAuditTrail
{
    public int Id { get; set; }
    public int EntregaTareaId { get; set; }
    public int DocenteId { get; set; }
    public decimal? NotaAnterior { get; set; }
    public decimal NotaNueva { get; set; }
    public string Razon { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation
    public EntregaTarea? EntregaTarea { get; set; }
    public Usuario? Docente { get; set; }
}
