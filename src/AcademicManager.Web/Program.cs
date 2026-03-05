using Serilog;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application;
using AcademicManager.Application.Services;
using AcademicManager.Application.Interfaces;
using AcademicManager.Infrastructure;
using AcademicManager.Infrastructure.Repositories;
using AcademicManager.Web.Components;
using AcademicManager.Web.Services;
using AcademicManager.Web.Services.Authentication;
using AcademicManager.Web.Services.Authorization;
using AcademicManager.Web.HealthChecks;
using Microsoft.AspNetCore.Mvc;

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

    builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.AddPolicy("AdminOnly", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("DocenteOrAdmin", policy =>
            policy.RequireRole("Admin", "Docente"));

        options.AddPolicy("AlumnoOnly", policy =>
            policy.RequireRole("Alumno"));

        options.AddPolicy("CanManageAlumnos", policy =>
            policy.RequireRole("Admin", "Docente"));

        options.AddPolicy("CanManageDocentes", policy =>
            policy.RequireRole("Admin"));

        options.AddPolicy("CanManageUsers", policy =>
            policy.RequireRole("Admin"));
    });

    builder.Services.AddAuthentication("SessionAuth")
        .AddCookie("SessionAuth", options =>
        {
            options.Cookie.Name = "AcademicManager.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
            options.ExpireTimeSpan = TimeSpan.FromHours(2);
            options.SlidingExpiration = true;
        });

    builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();

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
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
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
    app.UseSession();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapControllers();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.MapHealthChecks("/health");

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
