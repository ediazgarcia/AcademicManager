namespace AcademicManager.Domain.Entities;

public class DocenteCurso
{
    public int Id { get; set; }
    public int DocenteId { get; set; }
    public int CursoId { get; set; }
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
