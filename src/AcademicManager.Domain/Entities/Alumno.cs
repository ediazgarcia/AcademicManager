namespace AcademicManager.Domain.Entities;

public class Alumno
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;         // Matrícula / Código estudiantil
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Genero { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreApoderado { get; set; } = string.Empty;
    public string TelefonoApoderado { get; set; } = string.Empty;
    public int? GradoId { get; set; }
    public int? SeccionId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Navigation (no cargado por Dapper directamente)
    public Grado? Grado { get; set; }
    public Seccion? Seccion { get; set; }
}
