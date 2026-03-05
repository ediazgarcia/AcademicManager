using AcademicManager.Infrastructure.Data;
using AcademicManager.Application.Interfaces;
using AcademicManager.Application.Services;
using AcademicManager.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Dapper;

namespace AcademicManager.Infrastructure.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<DbConnectionFactory>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>();

        try
        {
            // Update schema if necessary
            using (var connection = connectionFactory.CreateConnection())
            {
                logger.LogInformation("Checking database schema updates...");
                
                // Add FechaCreacion to PeriodosAcademicos if missing
                const string checkColumnSql = @"
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('PeriodosAcademicos') 
                        AND name = 'FechaCreacion'
                    )
                    BEGIN
                        ALTER TABLE PeriodosAcademicos ADD FechaCreacion DATETIME DEFAULT GETDATE();
                    END";
                await connection.ExecuteAsync(checkColumnSql);

                // Add TwoFactorEnabled to Usuarios if missing
                const string checkTwoFactorEnabledSql = @"
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('Usuarios') 
                        AND name = 'TwoFactorEnabled'
                    )
                    BEGIN
                        ALTER TABLE Usuarios ADD TwoFactorEnabled BIT NOT NULL DEFAULT 0;
                    END";
                await connection.ExecuteAsync(checkTwoFactorEnabledSql);

                // Add TwoFactorSecret to Usuarios if missing
                const string checkTwoFactorSecretSql = @"
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('Usuarios') 
                        AND name = 'TwoFactorSecret'
                    )
                    BEGIN
                        ALTER TABLE Usuarios ADD TwoFactorSecret NVARCHAR(256) NULL;
                    END";
                await connection.ExecuteAsync(checkTwoFactorSecretSql);

                // Add MINERD expert-level columns to Planificaciones if missing
                string[] minerdColumns = new[]
                {
                    "Mes NVARCHAR(50) NULL DEFAULT ''",
                    "TituloUnidad NVARCHAR(500) NULL DEFAULT ''",
                    "SituacionAprendizaje NVARCHAR(MAX) NULL DEFAULT ''",
                    "CompetenciasFundamentales NVARCHAR(MAX) NULL DEFAULT ''",
                    "CompetenciasEspecificas NVARCHAR(MAX) NULL DEFAULT ''",
                    "ContenidosConceptuales NVARCHAR(MAX) NULL DEFAULT ''",
                    "ContenidosProcedimentales NVARCHAR(MAX) NULL DEFAULT ''",
                    "ContenidosActitudinales NVARCHAR(MAX) NULL DEFAULT ''",
                    "IndicadoresLogro NVARCHAR(MAX) NULL DEFAULT ''",
                    "EstrategiasEnsenanza NVARCHAR(MAX) NULL DEFAULT ''",
                    "ActividadesEvaluacion NVARCHAR(MAX) NULL DEFAULT ''",
                    "EjesTransversales NVARCHAR(MAX) NULL DEFAULT ''",
                    "AnoAcademico INT DEFAULT YEAR(GETDATE())",
                    "TipoplanificacionAcademico NVARCHAR(50) DEFAULT 'Anual'",
                    "CriteriosEvaluacion NVARCHAR(MAX) NULL DEFAULT ''",
                    "UsuarioAprobadorId INT NULL",
                    "FechaAprobacion DATETIME NULL",
                    "MotivoRechazo NVARCHAR(MAX) NULL"
                };

                foreach (var colDef in minerdColumns)
                {
                    var colName = colDef.Split(' ')[0];
                    var addColSql = $@"
                        IF NOT EXISTS (
                            SELECT 1 FROM sys.columns 
                            WHERE object_id = OBJECT_ID('Planificaciones') 
                            AND name = '{colName}'
                        )
                        BEGIN
                            ALTER TABLE Planificaciones ADD {colDef};
                        END";
                    await connection.ExecuteAsync(addColSql);
                }

                // Add new columns to PlanificacionesMensuales if missing
                string[] mensualColumns = new[]
                {
                    "PlanificacionId INT NULL",
                    "NumeroMes INT NOT NULL DEFAULT 1",
                    "EstrategiasEnsenanza NVARCHAR(MAX) NULL DEFAULT ''",
                    "RecursosDidacticos NVARCHAR(MAX) NULL DEFAULT ''",
                    "ActividadesEvaluacion NVARCHAR(MAX) NULL DEFAULT ''",
                    "EjesTransversales NVARCHAR(MAX) NULL DEFAULT ''",
                    "Observaciones NVARCHAR(MAX) NULL DEFAULT ''"
                };

                foreach (var colDef in mensualColumns)
                {
                    var colName = colDef.Split(' ')[0];
                    var addColSql = $@"
                        IF NOT EXISTS (
                            SELECT 1 FROM sys.columns 
                            WHERE object_id = OBJECT_ID('PlanificacionesMensuales') 
                            AND name = '{colName}'
                        )
                        BEGIN
                            ALTER TABLE PlanificacionesMensuales ADD {colDef};
                        END";
                    await connection.ExecuteAsync(addColSql);
                }
            }

            // Check if admin exists
            var admin = await usuarioRepository.GetByNombreUsuarioAsync("admin");
            if (admin == null)
            {
                logger.LogInformation("Creating default Super Admin user...");
                
                var superAdmin = new Usuario
                {
                    NombreUsuario = "admin",
                    Email = "admin@academicmanager.com",
                    PasswordHash = AuthService.HashPassword("admin123"), // You should change this after first login
                    Rol = "Admin",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await usuarioRepository.CreateAsync(superAdmin);
                logger.LogInformation("Super Admin created successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
