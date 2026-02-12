namespace AcademicManager.Domain.Entities;

public class Docente
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string Genero { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public string GradoAcademico { get; set; } = string.Empty; // Licenciado, Magíster, Doctor
    public DateTime FechaContratacion { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
}
