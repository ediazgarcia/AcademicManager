using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace AcademicManager.Web.Services.Authentication;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private const string TokenKey = "AuthToken";
    private const string RefreshTokenKey = "RefreshToken";

    public CustomAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        if (httpContext.User.Identity?.IsAuthenticated == true)
        {
            return Task.FromResult(new AuthenticationState(httpContext.User));
        }

        var userId = httpContext.Session.GetInt32("UserId");
        var username = httpContext.Session.GetString("UserName");
        var role = httpContext.Session.GetString("UserRole");
        if (!userId.HasValue || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var claims = new List<Claim>
        {
            new("userId", userId.Value.ToString()),
            new("username", username),
            new("role", role),
            new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return Task.FromResult(new AuthenticationState(user));
    }

    public Task MarkUserAsAuthenticated(string token, string? refreshToken = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return Task.CompletedTask;

        httpContext.Session.SetString(TokenKey, token);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            httpContext.Session.SetString(RefreshTokenKey, refreshToken);
        }

        var userId = httpContext.Session.GetInt32("UserId");
        var username = httpContext.Session.GetString("UserName");
        var role = httpContext.Session.GetString("UserRole");

        if (!userId.HasValue || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(role))
        {
            return Task.CompletedTask;
        }

        var claims = new List<Claim>
        {
            new("userId", userId.Value.ToString()),
            new("username", username),
            new("role", role),
            new(ClaimTypes.NameIdentifier, userId.Value.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        return Task.CompletedTask;
    }

    public Task MarkUserAsLoggedOut()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Session.Remove(TokenKey);
            httpContext.Session.Remove(RefreshTokenKey);
        }

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
        return Task.CompletedTask;
    }

    public string? GetToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetString(TokenKey);
    }

    public string? GetRefreshToken()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetString(RefreshTokenKey);
    }

    public bool IsAuthenticated()
    {
        var token = GetToken();
        return !string.IsNullOrEmpty(token);
    }

    public int? GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetInt32("UserId");
    }

    public string? GetUsername()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetString("UserName");
    }

    public string? GetUserRole()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Session.GetString("UserRole");
    }
}
