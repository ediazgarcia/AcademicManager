namespace AcademicManager.Application.DTOs;

public class AlumnoDto
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
    public string NombreApoderado { get; set; } = string.Empty;
    public string TelefonoApoderado { get; set; } = string.Empty;
    public int? GradoId { get; set; }
    public int? SeccionId { get; set; }
    public bool Activo { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? GradoNombre { get; set; }
    public string? SeccionNombre { get; set; }
    public string NombreCompleto => $"{Nombres} {Apellidos}";
}

public class CreateAlumnoDto
{
    public string Codigo { get; set; } = string.Empty;
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
}

public class UpdateAlumnoDto
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
    public string NombreApoderado { get; set; } = string.Empty;
    public string TelefonoApoderado { get; set; } = string.Empty;
    public int? GradoId { get; set; }
    public int? SeccionId { get; set; }
    public bool Activo { get; set; }
}
