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
