using Microsoft.AspNetCore.Mvc;
using AcademicManager.Application.Services;

namespace AcademicManager.Web.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly AuthService _authService;

    public AccountController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
    {
        var usuario = await _authService.LoginAsync(username, password);

        if (usuario == null)
        {
            return Redirect("/login?error=true");
        }

        // Set Session
        HttpContext.Session.SetInt32("UserId", usuario.Id);
        HttpContext.Session.SetString("UserName", usuario.NombreUsuario);
        HttpContext.Session.SetString("UserRole", usuario.Rol);
        if (!string.IsNullOrEmpty(usuario.Email))
        {
            HttpContext.Session.SetString("UserEmail", usuario.Email);
        }
        if (usuario.DocenteId.HasValue)
        {
            HttpContext.Session.SetInt32("DocenteId", usuario.DocenteId.Value);
        }

        return Redirect("/");
    }

    [HttpGet("Logout")]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return Redirect("/login");
    }
}
