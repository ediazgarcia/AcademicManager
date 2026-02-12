using AcademicManager.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<AlumnoService>();
        services.AddScoped<DocenteService>();
        services.AddScoped<PeriodoAcademicoService>();
        services.AddScoped<GradoService>();
        services.AddScoped<SeccionService>();
        services.AddScoped<CursoService>();
        services.AddScoped<HorarioService>();
        services.AddScoped<PlanificacionService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<EvaluacionService>();
        services.AddScoped<CalificacionService>();
        services.AddScoped<AsistenciaService>();
        services.AddScoped<SolicitudRegistroService>();

        return services;
    }
}
