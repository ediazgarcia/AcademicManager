using AcademicManager.Application.Mappings;
using AcademicManager.Application.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AcademicManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(config => config.AddProfile<MappingProfile>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<AuthService>();
        services.AddScoped<TwoFactorService>();
        services.AddScoped<AlumnoService>();
        services.AddScoped<AcademicWorkspaceService>();
        services.AddScoped<MatriculaCursoService>();
        services.AddScoped<DocenteCursoService>();
        services.AddScoped<DocenteService>();
        services.AddScoped<PeriodoAcademicoService>();
        services.AddScoped<GradoService>();
        services.AddScoped<SeccionService>();
        services.AddScoped<CursoService>();
        services.AddScoped<HorarioService>();
        services.AddScoped<PlanificacionValidationService>();
        services.AddScoped<PlanificacionService>();
        services.AddScoped<PlanificacionMensualService>();
        services.AddScoped<PlanificacionDiariaService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<EvaluacionService>();
        services.AddScoped<CalificacionService>();
        services.AddScoped<AsistenciaService>();
        services.AddScoped<SolicitudRegistroService>();
        services.AddScoped<TareaService>();
        services.AddScoped<AiService>();
        services.AddScoped<ExportService>();

        // Nuevos servicios de FASE 1
        services.AddScoped<ReportingService>();
        services.AddScoped<GradingUnifyService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<StudentProgressService>();
        services.AddScoped<FeedbackTemplateService>();
        services.AddScoped<AuditTrailService>();

        // Nuevos servicios de FASE 3
        services.AddScoped<CoordinadorService>();

        return services;
    }
}
