using Serilog;
using AcademicManager.Application;
using AcademicManager.Infrastructure;
using AcademicManager.Web.Components;
using AcademicManager.Web.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting AcademicManager Web application...");
    var builder = WebApplication.CreateBuilder(args);
    
    // Use Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add support for Controllers (for AccountController login)
builder.Services.AddControllers();

// Clean Architecture DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// Custom Services
builder.Services.AddSingleton<INotificationService, NotificationService>();
builder.Services.AddSingleton<IErrorHandler, ErrorHandler>();
builder.Services.AddSingleton<IValidationService, ValidationService>();

// Session / Auth state
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Seed Database
await AcademicManager.Infrastructure.Data.DbInitializer.InitializeAsync(app.Services);


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Ensure static files are served
app.UseSession(); // Enable Session Middleware BEFORE routing
app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers(); // Enable API controllers routing
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

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
