using Microsoft.AspNetCore.Components;
using AcademicManager.Web.Constants;

namespace AcademicManager.Web.Services.Authorization;

/// <summary>
/// Servicio centralizado para gestionar autorización basada en roles.
/// Proporciona métodos helper para validar permisos en Blazor.
/// </summary>
public class AuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// Obtiene el rol actual del usuario desde la sesión.
    /// </summary>
    public string GetCurrentRole()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return session?.GetString("UserRole") ?? string.Empty;
    }

    /// <summary>
    /// Obtiene el ID del usuario actual desde la sesión.
    /// </summary>
    public int GetCurrentUserId()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return session?.GetInt32("UserId") ?? 0;
    }

    /// <summary>
    /// Obtiene el ID del docente asociado al usuario actual (si aplica).
    /// </summary>
    public int GetCurrentDocenteId()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return session?.GetInt32("DocenteId") ?? 0;
    }

    /// <summary>
    /// Obtiene el nombre de usuario desde la sesión.
    /// </summary>
    public string GetCurrentUsername()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        return session?.GetString("UserName") ?? "Usuario";
    }

    /// <summary>
    /// Verifica si el usuario está autenticado.
    /// </summary>
    public bool IsAuthenticated()
    {
        var session = _httpContextAccessor.HttpContext?.Session;
        var userId = session?.GetInt32("UserId");
        var role = session?.GetString("UserRole");
        
        return userId.HasValue && !string.IsNullOrEmpty(role);
    }

    /// <summary>
    /// Verifica si el usuario tiene un rol específico.
    /// </summary>
    public bool HasRole(string requiredRole)
    {
        return GetCurrentRole() == requiredRole;
    }

    /// <summary>
    /// Verifica si el usuario tiene alguno de los roles especificados.
    /// </summary>
    public bool HasAnyRole(params string[] requiredRoles)
    {
        var currentRole = GetCurrentRole();
        return requiredRoles.Contains(currentRole);
    }

    /// <summary>
    /// Verifica si el usuario es administrador.
    /// </summary>
    public bool IsAdmin()
    {
        return HasRole(RoleConstants.ADMIN);
    }

    /// <summary>
    /// Verifica si el usuario es docente.
    /// </summary>
    public bool IsDocente()
    {
        return HasRole(RoleConstants.DOCENTE);
    }

    /// <summary>
    /// Verifica si el usuario es alumno.
    /// </summary>
    public bool IsAlumno()
    {
        return HasRole(RoleConstants.ALUMNO);
    }

    /// <summary>
    /// Verifica si el usuario es administrador o docente.
    /// </summary>
    public bool IsAdminOrDocente()
    {
        return HasAnyRole(RoleConstants.ADMIN, RoleConstants.DOCENTE);
    }

    /// <summary>
    /// Obtiene el rol actual con su descripción amigable.
    /// </summary>
    public string GetCurrentRoleDescription()
    {
        return RoleConstants.GetRoleDescription(GetCurrentRole());
    }

    /// <summary>
    /// Valida que el usuario tenga el rol requerido, lanzando una excepción si no.
    /// </summary>
    public void ValidateRole(string requiredRole)
    {
        if (!HasRole(requiredRole))
            throw new UnauthorizedAccessException(
                $"Se requiere el rol '{requiredRole}'. Tu rol actual es '{GetCurrentRole()}'."
            );
    }

    /// <summary>
    /// Valida que el usuario tenga alguno de los roles especificados.
    /// </summary>
    public void ValidateAnyRole(params string[] requiredRoles)
    {
        if (!HasAnyRole(requiredRoles))
            throw new UnauthorizedAccessException(
                $"Se requiere uno de los siguientes roles: {string.Join(", ", requiredRoles)}. " +
                $"Tu rol actual es '{GetCurrentRole()}'."
            );
    }
}
