using System.ComponentModel.DataAnnotations;

namespace AcademicManager.Domain.Entities;

public class SolicitudRegistro
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de usuario es requerido")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un rol")]
    public string RolSolicitado { get; set; } = "Alumno";

    public DateTime FechaSolicitud { get; set; } = DateTime.Now;

    public string Estado { get; set; } = "Pendiente"; // Pendiente, Aprobado, Rechazado

    public string? Mensaje { get; set; }
}
