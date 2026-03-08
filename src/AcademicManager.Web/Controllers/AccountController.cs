using System.Security.Claims;
using AcademicManager.Application.Services;
using AcademicManager.Domain.Entities;
using AcademicManager.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AcademicManager.Web.Controllers;

[Authorize]
[Route("[controller]")]
public class AccountController : Controller
{
    private const string PendingTwoFactorUserIdKey = "PendingTwoFactorUserId";
    private const string TwoFactorPendingKey = "TwoFactorPending";
    private const string TwoFactorSecretKey = "TwoFactorSecret";
    private const string UserIdKey = "UserId";
    private const string UserNameKey = "UserName";
    private const string UserRoleKey = "UserRole";
    private const string UserEmailKey = "UserEmail";
    private const string DocenteIdKey = "DocenteId";
    private const string AlumnoIdKey = "AlumnoId";
    private const string AuthTokenKey = "AuthToken";
    private const string RefreshTokenKey = "RefreshToken";
    private const string TwoFactorEnabledKey = "TwoFactorEnabled";
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    private readonly SolicitudRegistroService _solicitudRegistroService;

    public AccountController(
        AuthService authService,
        JwtService jwtService,
        SolicitudRegistroService solicitudRegistroService)
    {
        _authService = authService;
        _jwtService = jwtService;
        _solicitudRegistroService = solicitudRegistroService;
    }

    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, [FromForm] bool remember = false)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return Redirect("/login?error=true");
        }

        var (success, requiresTwoFactor, usuario) = await _authService.LoginAsync(username, password);

        if (!success || usuario is null)
        {
            return Redirect("/login?error=true");
        }

        if (requiresTwoFactor)
        {
            ClearPendingTwoFactorState();
            HttpContext.Session.SetInt32(PendingTwoFactorUserIdKey, usuario.Id);
            HttpContext.Session.SetString(TwoFactorPendingKey, "true");
            return Redirect("/login?twofactor=true");
        }

        await SetSessionValuesAsync(usuario, remember);
        return Redirect("/");
    }

    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    [HttpPost("Register")]
    public async Task<IActionResult> Register(
        [FromForm] string username,
        [FromForm] string email,
        [FromForm] string password,
        [FromForm] string role)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Redirect("/register?error=El%20usuario%20es%20requerido");
        }

        var passwordValidation = AuthService.ValidarComplejidadPassword(password);
        if (!passwordValidation.Valid)
        {
            return Redirect($"/register?error={Uri.EscapeDataString(passwordValidation.Message)}");
        }

        if (!string.Equals(role, "Alumno", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, "Docente", StringComparison.OrdinalIgnoreCase))
        {
            return Redirect("/register?error=Selecciona%20un%20tipo%20valido");
        }

        try
        {
            var solicitud = new SolicitudRegistro
            {
                NombreUsuario = username.Trim(),
                Email = email?.Trim() ?? string.Empty,
                RolSolicitado = role
            };

            await _solicitudRegistroService.CrearSolicitudAsync(solicitud, password);
            return Redirect("/register?success=true");
        }
        catch (Exception ex)
        {
            return Redirect($"/register?error={Uri.EscapeDataString(ex.Message)}");
        }
    }

    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    [HttpPost("VerifyTwoFactor")]
    public async Task<IActionResult> VerifyTwoFactor([FromForm] string code, [FromForm] bool remember = false)
    {
        var pendingUserId = HttpContext.Session.GetInt32(PendingTwoFactorUserIdKey);
        if (!pendingUserId.HasValue || !IsTwoFactorCodeValid(code))
        {
            ClearPendingTwoFactorState();
            return Redirect("/login?error=true");
        }

        var usuario = await _authService.ValidateTwoFactorAsync(pendingUserId.Value, code);
        ClearPendingTwoFactorState();

        if (usuario is null)
        {
            return Redirect("/login?twofactor=true&error=true");
        }

        await SetSessionValuesAsync(usuario, remember);
        return Redirect("/");
    }

    [HttpGet("TwoFactorSetup")]
    public async Task<IActionResult> TwoFactorSetup()
    {
        var userId = HttpContext.Session.GetInt32(UserIdKey);
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        try
        {
            var (secret, authUrl) = await _authService.GenerateTwoFactorSetupAsync(userId.Value);
            HttpContext.Session.SetString(TwoFactorSecretKey, secret);
            return Ok(new { secret, authUrl });
        }
        catch
        {
            return BadRequest(new { message = "Error al generar configuración 2FA" });
        }
    }

    [HttpPost("EnableTwoFactor")]
    public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest request)
    {
        var userId = HttpContext.Session.GetInt32(UserIdKey);
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        if (!IsTwoFactorCodeValid(request.Code))
        {
            return BadRequest(new { message = "Código inválido" });
        }

        var secret = HttpContext.Session.GetString(TwoFactorSecretKey);
        if (string.IsNullOrEmpty(secret))
        {
            return BadRequest(new { message = "Debe generar la configuración primero" });
        }

        var result = await _authService.EnableTwoFactorAsync(userId.Value, secret, request.Code);

        HttpContext.Session.Remove(TwoFactorSecretKey);

        if (result)
        {
            HttpContext.Session.SetString(TwoFactorEnabledKey, true.ToString());
            return Ok(new { message = "2FA habilitado exitosamente" });
        }

        return BadRequest(new { message = "Código inválido" });
    }

    [HttpPost("DisableTwoFactor")]
    public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest request)
    {
        var userId = HttpContext.Session.GetInt32(UserIdKey);
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "La contraseña es requerida" });
        }

        var result = await _authService.DisableTwoFactorAsync(userId.Value, request.Password);

        if (result)
        {
            HttpContext.Session.SetString(TwoFactorEnabledKey, false.ToString());
            return Ok(new { message = "2FA deshabilitado exitosamente" });
        }

        return BadRequest(new { message = "Contraseña incorrecta" });
    }

    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    [HttpPost("Api/Login")]
    public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        var (success, requiresTwoFactor, usuario) = await _authService.LoginAsync(request.Username, request.Password);

        if (!success || usuario is null)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        if (requiresTwoFactor)
        {
            ClearPendingTwoFactorState();
            HttpContext.Session.SetInt32(PendingTwoFactorUserIdKey, usuario.Id);
            HttpContext.Session.SetString(TwoFactorPendingKey, "true");
            return Ok(new
            {
                requiresTwoFactor = true,
                userId = usuario.Id,
                message = "Se requiere código 2FA"
            });
        }

        await SetSessionValuesAsync(usuario);

        var token = HttpContext.Session.GetString(AuthTokenKey)!;
        var refreshToken = HttpContext.Session.GetString(RefreshTokenKey)!;

        return Ok(new
        {
            token,
            refreshToken,
            twoFactorEnabled = usuario.TwoFactorEnabled,
            user = new
            {
                usuario.Id,
                usuario.NombreUsuario,
                usuario.Rol,
                usuario.Email
            }
        });
    }

    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    [HttpPost("Api/VerifyTwoFactor")]
    public async Task<IActionResult> ApiVerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        if (!IsTwoFactorCodeValid(request.Code))
        {
            return Unauthorized(new { message = "Código inválido" });
        }

        var pendingUserId = HttpContext.Session.GetInt32(PendingTwoFactorUserIdKey);
        if (!pendingUserId.HasValue)
        {
            return Unauthorized(new { message = "No hay autenticación 2FA pendiente" });
        }

        if (request.UserId > 0 && request.UserId != pendingUserId.Value)
        {
            ClearPendingTwoFactorState();
            return Unauthorized(new { message = "Solicitud 2FA inválida" });
        }

        var usuario = await _authService.ValidateTwoFactorAsync(pendingUserId.Value, request.Code);
        ClearPendingTwoFactorState();

        if (usuario is null)
        {
            return Unauthorized(new { message = "Código inválido" });
        }

        await SetSessionValuesAsync(usuario);

        var token = HttpContext.Session.GetString(AuthTokenKey)!;
        var refreshToken = HttpContext.Session.GetString(RefreshTokenKey)!;

        return Ok(new
        {
            token,
            refreshToken,
            twoFactorEnabled = usuario.TwoFactorEnabled,
            user = new
            {
                usuario.Id,
                usuario.NombreUsuario,
                usuario.Rol,
                usuario.Email
            }
        });
    }

    [AllowAnonymous]
    [EnableRateLimiting("AuthEndpoints")]
    [HttpPost("Api/Refresh")]
    public IActionResult RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Unauthorized(new { message = "Refresh token inválido" });
        }

        var refreshToken = HttpContext.Session.GetString(RefreshTokenKey);
        if (string.IsNullOrEmpty(refreshToken) || refreshToken != request.RefreshToken)
        {
            return Unauthorized(new { message = "Refresh token inválido" });
        }

        var userId = HttpContext.Session.GetInt32(UserIdKey);
        var username = HttpContext.Session.GetString(UserNameKey);
        var role = HttpContext.Session.GetString(UserRoleKey);
        var email = HttpContext.Session.GetString(UserEmailKey);
        var docenteId = HttpContext.Session.GetInt32(DocenteIdKey);

        if (!userId.HasValue || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
        {
            return Unauthorized(new { message = "Sesión inválida" });
        }

        var newToken = _jwtService.GenerateToken(
            userId.Value,
            username,
            role,
            email,
            docenteId);

        var newRefreshToken = _jwtService.GenerateRefreshToken();

        HttpContext.Session.SetString(AuthTokenKey, newToken);
        HttpContext.Session.SetString(RefreshTokenKey, newRefreshToken);

        return Ok(new
        {
            token = newToken,
            refreshToken = newRefreshToken
        });
    }

    [HttpGet("Logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("SessionAuth");
        HttpContext.Session.Clear();
        return Redirect("/login");
    }

    [HttpPost("Api/Logout")]
    public async Task<IActionResult> ApiLogout()
    {
        await HttpContext.SignOutAsync("SessionAuth");
        HttpContext.Session.Clear();
        return Ok(new { message = "Logout exitoso" });
    }

    [AllowAnonymous]
    [HttpGet("Api/CheckAuth")]
    public IActionResult CheckAuth()
    {
        var token = HttpContext.Session.GetString(AuthTokenKey);
        var userId = HttpContext.Session.GetInt32(UserIdKey);
        var username = HttpContext.Session.GetString(UserNameKey);
        var role = HttpContext.Session.GetString(UserRoleKey);

        if (string.IsNullOrEmpty(token) || !userId.HasValue)
        {
            return Unauthorized(new { authenticated = false });
        }

        if (_jwtService.IsTokenExpired(token))
        {
            return Unauthorized(new { authenticated = false, message = "Token expirado" });
        }

        return Ok(new
        {
            authenticated = true,
            user = new
            {
                userId,
                username,
                role
            }
        });
    }

    private async Task SetSessionValuesAsync(Usuario usuario, bool remember = false)
    {
        await HttpContext.SignOutAsync("SessionAuth");
        HttpContext.Session.Clear();

        var token = _jwtService.GenerateToken(
            usuario.Id,
            usuario.NombreUsuario,
            usuario.Rol,
            usuario.Email,
            usuario.DocenteId);

        var refreshToken = _jwtService.GenerateRefreshToken();

        HttpContext.Session.SetInt32(UserIdKey, usuario.Id);
        HttpContext.Session.SetString(UserNameKey, usuario.NombreUsuario);
        HttpContext.Session.SetString(UserRoleKey, usuario.Rol);
        HttpContext.Session.SetString(AuthTokenKey, token);
        HttpContext.Session.SetString(RefreshTokenKey, refreshToken);
        HttpContext.Session.SetString(TwoFactorEnabledKey, usuario.TwoFactorEnabled.ToString());

        if (!string.IsNullOrEmpty(usuario.Email))
        {
            HttpContext.Session.SetString(UserEmailKey, usuario.Email);
        }

        if (usuario.DocenteId.HasValue)
        {
            HttpContext.Session.SetInt32(DocenteIdKey, usuario.DocenteId.Value);
        }

        if (usuario.AlumnoId.HasValue)
        {
            HttpContext.Session.SetInt32(AlumnoIdKey, usuario.AlumnoId.Value);
        }

        var authenticationProperties = new AuthenticationProperties
        {
            IsPersistent = remember,
            AllowRefresh = true,
            ExpiresUtc = remember
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(2)
        };

        await HttpContext.SignInAsync("SessionAuth", BuildClaimsPrincipal(usuario), authenticationProperties);
    }

    private void ClearPendingTwoFactorState()
    {
        HttpContext.Session.Remove(PendingTwoFactorUserIdKey);
        HttpContext.Session.Remove(TwoFactorPendingKey);
    }

    private static ClaimsPrincipal BuildClaimsPrincipal(Usuario usuario)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, usuario.NombreUsuario),
            new(ClaimTypes.Role, usuario.Rol),
            new("userId", usuario.Id.ToString()),
            new("username", usuario.NombreUsuario),
            new("role", usuario.Rol)
        };

        if (!string.IsNullOrWhiteSpace(usuario.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, usuario.Email));
            claims.Add(new Claim("email", usuario.Email));
        }

        if (usuario.DocenteId.HasValue)
        {
            claims.Add(new Claim("docenteId", usuario.DocenteId.Value.ToString()));
        }

        if (usuario.AlumnoId.HasValue)
        {
            claims.Add(new Claim("alumnoId", usuario.AlumnoId.Value.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "SessionAuth");
        return new ClaimsPrincipal(identity);
    }

    private static bool IsTwoFactorCodeValid(string code) =>
        code.Length == 6 && code.All(char.IsDigit);
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class VerifyTwoFactorRequest
{
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class EnableTwoFactorRequest
{
    public string Code { get; set; } = string.Empty;
}

public class DisableTwoFactorRequest
{
    public string Password { get; set; } = string.Empty;
}
