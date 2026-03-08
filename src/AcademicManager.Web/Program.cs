using Serilog;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using AcademicManager.Application.Configuration;
using AcademicManager.Application;
using AcademicManager.Application.Services;
using AcademicManager.Application.Interfaces;
using AcademicManager.Infrastructure;
using AcademicManager.Infrastructure.Repositories;
using AcademicManager.Web.Components;
using AcademicManager.Web.Configuration;
using AcademicManager.Web.Constants;
using AcademicManager.Web.Services;
using AcademicManager.Web.Services.Authentication;
using AcademicManager.Web.Services.Authorization;
using AcademicManager.Web.HealthChecks;
using System.Threading.RateLimiting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting AcademicManager Web application...");
    var builder = WebApplication.CreateBuilder(args);
    
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddControllers();
    
    builder.Services.AddHealthChecks();

    builder.Services.AddOptions<JwtOptions>()
        .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
        .ValidateDataAnnotations()
        .Validate(options => !string.Equals(options.Key, "__SET_IN_ENV__", StringComparison.Ordinal),
            "Jwt:Key debe configurarse desde variables de entorno o secretos de usuario.")
        .ValidateOnStart();

    builder.Services.AddOptions<GeminiOptions>()
        .Bind(builder.Configuration.GetSection(GeminiOptions.SectionName));

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole(RoleConstants.ADMIN));

        options.AddPolicy("CoordinatorOrAdmin", policy =>
            policy.RequireRole(RoleConstants.ADMIN, RoleConstants.COORDINADOR));

        options.AddPolicy("DocenteOrAdmin", policy =>
            policy.RequireRole(RoleConstants.ADMIN, RoleConstants.COORDINADOR, RoleConstants.DOCENTE));

        options.AddPolicy("AlumnoOnly", policy =>
            policy.RequireRole(RoleConstants.ALUMNO));

        options.AddPolicy("CanManageAlumnos", policy =>
            policy.RequireRole(RoleConstants.ADMIN, RoleConstants.COORDINADOR, RoleConstants.DOCENTE));

        options.AddPolicy("CanManageDocentes", policy =>
            policy.RequireRole(RoleConstants.ADMIN, RoleConstants.COORDINADOR));

        options.AddPolicy("CanManageUsers", policy =>
            policy.RequireRole(RoleConstants.ADMIN, RoleConstants.COORDINADOR));
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("AuthEndpoints", httpContext =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    AutoReplenishment = true
                }));
    });

    builder.Services.AddAuthentication("SessionAuth")
        .AddCookie("SessionAuth", options =>
        {
            options.Cookie.Name = "AcademicManager.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromHours(2);
            options.SlidingExpiration = true;
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api")
                        || context.Request.Path.StartsWithSegments("/Account/Api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api")
                        || context.Request.Path.StartsWithSegments("/Account/Api"))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                }
            };
        });

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure();

    builder.Services.AddSingleton<JwtService>();



    builder.Services.AddScoped<AuthorizationService>();
    builder.Services.AddScoped<CustomAuthenticationStateProvider>();
    builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());

    builder.Services.AddSingleton<IErrorHandler, ErrorHandler>();
    builder.Services.AddSingleton<IValidationService, ValidationService>();

    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(2);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.Name = "AcademicManager.Session";
    });
    builder.Services.AddHttpContextAccessor();

    builder.Services.AddHttpClient();

    var app = builder.Build();

    await AcademicManager.Infrastructure.Data.DbInitializer.InitializeAsync(app.Services);

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRateLimiter();
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapControllers();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.MapHealthChecks("/health").AllowAnonymous();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
