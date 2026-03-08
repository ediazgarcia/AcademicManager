namespace AcademicManager.Web.Constants;

/// <summary>
/// Constantes de roles de usuario en el sistema AcademicManager.
/// Defines los roles y sus permisos a nivel de aplicación.
/// </summary>
public static class RoleConstants
{
    // Definición de roles
    public const string ADMIN = "Admin";
    public const string COORDINADOR = "Coordinador";
    public const string DOCENTE = "Docente";
    public const string ALUMNO = "Alumno";

    // Arrays de roles para políticas
    public static readonly string[] AdminRoles = [ADMIN];
    public static readonly string[] CoordinacionRoles = [ADMIN, COORDINADOR];
    public static readonly string[] DocenteRoles = [DOCENTE];
    public static readonly string[] AlumnoRoles = [ALUMNO];
    public static readonly string[] AdminAndDocenteRoles = [ADMIN, COORDINADOR, DOCENTE];
    public static readonly string[] AllRoles = [ADMIN, COORDINADOR, DOCENTE, ALUMNO];

    // Descripciones de roles
    public static readonly Dictionary<string, string> RoleDescriptions = new()
    {
        { ADMIN, "Administrador - Control total del sistema" },
        { COORDINADOR, "Coordinador - Supervisa docentes, planificaciones y seguimiento estudiantil" },
        { DOCENTE, "Docente - Gestiona planificacion, estudiantes y evaluacion diaria" },
        { ALUMNO, "Alumno - Realiza tareas y entregas" }
    };

    /// <summary>
    /// Obtiene la descripción de un rol.
    /// </summary>
    public static string GetRoleDescription(string? role)
    {
        return string.IsNullOrEmpty(role) || !RoleDescriptions.ContainsKey(role)
            ? "Rol desconocido"
            : RoleDescriptions[role];
    }

    /// <summary>
    /// Valida si un rol es válido.
    /// </summary>
    public static bool IsValidRole(string? role)
    {
        return !string.IsNullOrEmpty(role) && 
               (role == ADMIN || role == COORDINADOR || role == DOCENTE || role == ALUMNO);
    }

    public static bool IsCoordinatorLike(string? role) =>
        string.Equals(role, ADMIN, StringComparison.OrdinalIgnoreCase) ||
        string.Equals(role, COORDINADOR, StringComparison.OrdinalIgnoreCase);
}
