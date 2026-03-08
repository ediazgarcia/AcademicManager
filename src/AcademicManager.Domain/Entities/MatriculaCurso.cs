namespace AcademicManager.Domain.Entities;

public class MatriculaCurso
{
    public int Id { get; set; }
    public int AlumnoId { get; set; }
    public int CursoId { get; set; }
    public DateTime FechaMatricula { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;
}
