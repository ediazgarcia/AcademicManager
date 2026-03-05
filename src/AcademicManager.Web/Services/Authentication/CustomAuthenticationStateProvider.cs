using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

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

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var token = httpContext.Session.GetString(TokenKey);
        if (string.IsNullOrEmpty(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var claims = ParseClaimsFromJwt(token);
        if (claims == null || !claims.Any())
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        return new AuthenticationState(user);
    }

    public async Task MarkUserAsAuthenticated(string token, string? refreshToken = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return;

        httpContext.Session.SetString(TokenKey, token);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            httpContext.Session.SetString(RefreshTokenKey, refreshToken);
        }

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            httpContext.Session.Remove(TokenKey);
            httpContext.Session.Remove(RefreshTokenKey);
        }

        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
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
        var userIdStr = httpContext?.Session.GetString("UserId");
        if (int.TryParse(userIdStr, out var userId))
        {
            return userId;
        }
        return null;
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

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string token)
    {
        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            return jwtToken.Claims.ToList();
        }
        catch
        {
            return null;
        }
    }
}
