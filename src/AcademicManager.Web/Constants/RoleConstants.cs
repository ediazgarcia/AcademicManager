namespace AcademicManager.Web.Constants;

/// <summary>
/// Constantes de roles de usuario en el sistema AcademicManager.
/// Defines los roles y sus permisos a nivel de aplicación.
/// </summary>
public static class RoleConstants
{
    // Definición de roles
    public const string ADMIN = "Admin";
    public const string DOCENTE = "Docente";
    public const string ALUMNO = "Alumno";

    // Arrays de roles para políticas
    public static readonly string[] AdminRoles = [ADMIN];
    public static readonly string[] DocenteRoles = [DOCENTE];
    public static readonly string[] AlumnoRoles = [ALUMNO];
    public static readonly string[] AdminAndDocenteRoles = [ADMIN, DOCENTE];
    public static readonly string[] AllRoles = [ADMIN, DOCENTE, ALUMNO];

    // Descripciones de roles
    public static readonly Dictionary<string, string> RoleDescriptions = new()
    {
        { ADMIN, "Administrador - Control total del sistema" },
        { DOCENTE, "Docente - Gestiona tareas y calificaciones" },
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
               (role == ADMIN || role == DOCENTE || role == ALUMNO);
    }
}
