using Microsoft.AspNetCore.Mvc;
using AcademicManager.Application.Services;
using AcademicManager.Web.Services;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Web.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly AuthService _authService;
    private readonly JwtService _jwtService;
    public AccountController(
        AuthService authService,
        JwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password, [FromForm] bool remember = false)
    {
        var (success, requiresTwoFactor, usuario) = await _authService.LoginAsync(username, password);

        if (!success)
        {
            return Redirect("/login?error=true");
        }

        if (requiresTwoFactor)
        {
            HttpContext.Session.SetInt32("PendingTwoFactorUserId", usuario!.Id);
            HttpContext.Session.SetString("TwoFactorPending", "true");
            return Redirect("/login?twofactor=true");
        }

        await SetSessionValuesAsync(usuario!);
        
        if (remember)
        {
            HttpContext.Session.SetString("RememberMe", "true");
        }

        return Redirect("/");
    }

    [HttpPost("VerifyTwoFactor")]
    public async Task<IActionResult> VerifyTwoFactor([FromForm] string code, [FromForm] bool remember = false)
    {
        var pendingUserId = HttpContext.Session.GetInt32("PendingTwoFactorUserId");
        if (!pendingUserId.HasValue)
        {
            HttpContext.Session.Remove("PendingTwoFactorUserId");
            HttpContext.Session.Remove("TwoFactorPending");
            return Redirect("/login?error=true");
        }

        var usuario = await _authService.ValidateTwoFactorAsync(pendingUserId.Value, code);
        
        HttpContext.Session.Remove("PendingTwoFactorUserId");
        HttpContext.Session.Remove("TwoFactorPending");

        if (usuario == null)
        {
            return Redirect("/login?twofactor=true&error=true");
        }

        await SetSessionValuesAsync(usuario);
        
        if (remember)
        {
            HttpContext.Session.SetString("RememberMe", "true");
        }

        return Redirect("/");
    }

    [HttpGet("TwoFactorSetup")]
    public async Task<IActionResult> TwoFactorSetup()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return Redirect("/login");
        }

        try
        {
            var (secret, authUrl) = await _authService.GenerateTwoFactorSetupAsync(userId.Value);
            HttpContext.Session.SetString("TwoFactorSecret", secret);
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
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var secret = HttpContext.Session.GetString("TwoFactorSecret");
        if (string.IsNullOrEmpty(secret))
        {
            return BadRequest(new { message = "Debe generar la configuración primero" });
        }

        var result = await _authService.EnableTwoFactorAsync(userId.Value, secret, request.Code);
        
        HttpContext.Session.Remove("TwoFactorSecret");

        if (result)
        {
            return Ok(new { message = "2FA habilitado exitosamente" });
        }

        return BadRequest(new { message = "Código inválido" });
    }

    [HttpPost("DisableTwoFactor")]
    public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest request)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var result = await _authService.DisableTwoFactorAsync(userId.Value, request.Password);
        
        if (result)
        {
            return Ok(new { message = "2FA deshabilitado exitosamente" });
        }

        return BadRequest(new { message = "Contraseña incorrecta" });
    }

    private async Task SetSessionValuesAsync(Usuario usuario)
    {
        var token = _jwtService.GenerateToken(
            usuario.Id,
            usuario.NombreUsuario,
            usuario.Rol,
            usuario.Email,
            usuario.DocenteId
        );

        var refreshToken = _jwtService.GenerateRefreshToken();

        HttpContext.Session.SetInt32("UserId", usuario.Id);
        HttpContext.Session.SetString("UserName", usuario.NombreUsuario);
        HttpContext.Session.SetString("UserRole", usuario.Rol);
        HttpContext.Session.SetString("AuthToken", token);
        HttpContext.Session.SetString("RefreshToken", refreshToken);
        HttpContext.Session.SetString("TwoFactorEnabled", usuario.TwoFactorEnabled.ToString());

        if (!string.IsNullOrEmpty(usuario.Email))
        {
            HttpContext.Session.SetString("UserEmail", usuario.Email);
        }
        if (usuario.DocenteId.HasValue)
        {
            HttpContext.Session.SetInt32("DocenteId", usuario.DocenteId.Value);
        }

        await Task.CompletedTask;
    }

    [HttpPost("Api/Login")]
    public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
    {
        var (success, requiresTwoFactor, usuario) = await _authService.LoginAsync(request.Username, request.Password);

        if (!success)
        {
            return Unauthorized(new { message = "Usuario o contraseña incorrectos" });
        }

        if (requiresTwoFactor)
        {
            return Ok(new { requiresTwoFactor = true, message = "Se requiere código 2FA" });
        }

        var token = _jwtService.GenerateToken(
            usuario!.Id,
            usuario.NombreUsuario,
            usuario.Rol,
            usuario.Email,
            usuario.DocenteId
        );

        var refreshToken = _jwtService.GenerateRefreshToken();

        HttpContext.Session.SetInt32("UserId", usuario.Id);
        HttpContext.Session.SetString("UserName", usuario.NombreUsuario);
        HttpContext.Session.SetString("UserRole", usuario.Rol);
        HttpContext.Session.SetString("AuthToken", token);
        HttpContext.Session.SetString("RefreshToken", refreshToken);
        HttpContext.Session.SetString("TwoFactorEnabled", usuario.TwoFactorEnabled.ToString());

        if (!string.IsNullOrEmpty(usuario.Email))
        {
            HttpContext.Session.SetString("UserEmail", usuario.Email);
        }
        if (usuario.DocenteId.HasValue)
        {
            HttpContext.Session.SetInt32("DocenteId", usuario.DocenteId.Value);
        }

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

    [HttpPost("Api/VerifyTwoFactor")]
    public async Task<IActionResult> ApiVerifyTwoFactor([FromBody] VerifyTwoFactorRequest request)
    {
        var usuario = await _authService.ValidateTwoFactorAsync(request.UserId, request.Code);
        
        if (usuario == null)
        {
            return Unauthorized(new { message = "Código inválido" });
        }

        var token = _jwtService.GenerateToken(
            usuario.Id,
            usuario.NombreUsuario,
            usuario.Rol,
            usuario.Email,
            usuario.DocenteId
        );

        var refreshToken = _jwtService.GenerateRefreshToken();

        HttpContext.Session.SetInt32("UserId", usuario.Id);
        HttpContext.Session.SetString("UserName", usuario.NombreUsuario);
        HttpContext.Session.SetString("UserRole", usuario.Rol);
        HttpContext.Session.SetString("AuthToken", token);
        HttpContext.Session.SetString("RefreshToken", refreshToken);
        HttpContext.Session.SetString("TwoFactorEnabled", usuario.TwoFactorEnabled.ToString());

        if (!string.IsNullOrEmpty(usuario.Email))
        {
            HttpContext.Session.SetString("UserEmail", usuario.Email);
        }
        if (usuario.DocenteId.HasValue)
        {
            HttpContext.Session.SetInt32("DocenteId", usuario.DocenteId.Value);
        }

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

    [HttpPost("Api/Refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var refreshToken = HttpContext.Session.GetString("RefreshToken");
        if (string.IsNullOrEmpty(refreshToken) || refreshToken != request.RefreshToken)
        {
            return Unauthorized(new { message = "Refresh token inválido" });
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        var username = HttpContext.Session.GetString("UserName");
        var role = HttpContext.Session.GetString("UserRole");

        if (!userId.HasValue || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(role))
        {
            return Unauthorized(new { message = "Sesión inválida" });
        }

        var newToken = _jwtService.GenerateToken(
            userId.Value,
            username,
            role
        );

        var newRefreshToken = _jwtService.GenerateRefreshToken();

        HttpContext.Session.SetString("AuthToken", newToken);
        HttpContext.Session.SetString("RefreshToken", newRefreshToken);

        return Ok(new
        {
            token = newToken,
            refreshToken = newRefreshToken
        });
    }

    [HttpGet("Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Redirect("/login");
    }

    [HttpPost("Api/Logout")]
    public IActionResult ApiLogout()
    {
        HttpContext.Session.Clear();
        return Ok(new { message = "Logout exitoso" });
    }

    [HttpGet("Api/CheckAuth")]
    public IActionResult CheckAuth()
    {
        var token = HttpContext.Session.GetString("AuthToken");
        var userId = HttpContext.Session.GetInt32("UserId");
        var username = HttpContext.Session.GetString("UserName");
        var role = HttpContext.Session.GetString("UserRole");

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
