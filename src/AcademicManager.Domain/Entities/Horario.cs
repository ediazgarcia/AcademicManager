namespace AcademicManager.Domain.Entities;

public class Horario
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public int DocenteId { get; set; }
    public int SeccionId { get; set; }
    public int PeriodoAcademicoId { get; set; }
    public string DiaSemana { get; set; } = string.Empty;   // Lunes, Martes, ...
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public string Aula { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    // Navigation
    public Curso? Curso { get; set; }
    public Docente? Docente { get; set; }
    public Seccion? Seccion { get; set; }
    public PeriodoAcademico? PeriodoAcademico { get; set; }
}
