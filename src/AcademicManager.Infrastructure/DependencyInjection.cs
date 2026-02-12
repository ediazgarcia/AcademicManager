using AcademicManager.Application.Interfaces;
using AcademicManager.Infrastructure.Data;
using AcademicManager.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Database
        services.AddSingleton<DbConnectionFactory>();

        // Repositories
        services.AddScoped<IUsuarioRepository, DapperUsuarioRepository>();
        services.AddScoped<IAlumnoRepository, DapperAlumnoRepository>();
        services.AddScoped<IDocenteRepository, DapperDocenteRepository>();
        services.AddScoped<IPeriodoAcademicoRepository, DapperPeriodoAcademicoRepository>();
        services.AddScoped<IGradoRepository, DapperGradoRepository>();
        services.AddScoped<ISeccionRepository, DapperSeccionRepository>();
        services.AddScoped<ICursoRepository, DapperCursoRepository>();
        services.AddScoped<IHorarioRepository, DapperHorarioRepository>();
        services.AddScoped<IPlanificacionRepository, DapperPlanificacionRepository>();
        services.AddScoped<IEvaluacionRepository, DapperEvaluacionRepository>();
        services.AddScoped<ICalificacionRepository, DapperCalificacionRepository>();
        services.AddScoped<IAsistenciaRepository, DapperAsistenciaRepository>();
        services.AddScoped<ISolicitudRegistroRepository, DapperSolicitudRegistroRepository>();


        return services;
    }
}
