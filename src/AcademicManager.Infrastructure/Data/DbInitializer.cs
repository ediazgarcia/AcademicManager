using System.Text.RegularExpressions;
using Dapper;
using AcademicManager.Application.Interfaces;
using AcademicManager.Application.Services;
using AcademicManager.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AcademicManager.Infrastructure.Data;

public class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var usuarioRepository = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
        var connectionFactory = scope.ServiceProvider.GetRequiredService<DbConnectionFactory>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var environment = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbInitializer>>();

        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                logger.LogWarning("Connection string 'DefaultConnection' is empty. Database initialization was skipped.");
                return;
            }

            await EnsureDatabaseExistsAsync(connectionString, logger);
            await EnsureSchemaBootstrappedAsync(connectionFactory, environment.ContentRootPath, logger);
            await ApplySchemaUpdatesAsync(connectionFactory, logger);

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

    private static async Task EnsureDatabaseExistsAsync(string connectionString, ILogger<DbInitializer> logger)
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var dbName = builder.InitialCatalog;

        if (string.IsNullOrWhiteSpace(dbName))
        {
            logger.LogWarning("DefaultConnection does not contain a database name (Initial Catalog).");
            return;
        }

        var masterBuilder = new SqlConnectionStringBuilder(connectionString)
        {
            InitialCatalog = "master"
        };

        await using var masterConnection = new SqlConnection(masterBuilder.ConnectionString);
        await masterConnection.OpenAsync();

        var databaseExists = await masterConnection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM sys.databases WHERE name = @DbName;",
            new { DbName = dbName });

        if (databaseExists > 0)
        {
            return;
        }

        logger.LogInformation("Database '{DatabaseName}' was not found. Creating it automatically...", dbName);

        var escapedDbName = dbName.Replace("]", "]]", StringComparison.Ordinal);
        await masterConnection.ExecuteAsync($"CREATE DATABASE [{escapedDbName}];");

        logger.LogInformation("Database '{DatabaseName}' created successfully.", dbName);
    }

    private static async Task EnsureSchemaBootstrappedAsync(
        DbConnectionFactory connectionFactory,
        string contentRootPath,
        ILogger<DbInitializer> logger)
    {
        using var connection = connectionFactory.CreateConnection();
        const string schemaCheckSql = "SELECT OBJECT_ID('dbo.Usuarios', 'U');";
        var usersTableObjectId = await connection.ExecuteScalarAsync<int?>(schemaCheckSql);

        if (usersTableObjectId.HasValue)
        {
            return;
        }

        var bootstrapScriptPath = ResolveBootstrapScriptPath(contentRootPath);
        if (bootstrapScriptPath is null)
        {
            logger.LogWarning("Base schema is missing and bootstrap script 'database/db.sql' could not be resolved.");
            return;
        }

        logger.LogInformation("Base schema not found. Executing bootstrap script from '{ScriptPath}'.", bootstrapScriptPath);

        var scriptContent = await File.ReadAllTextAsync(bootstrapScriptPath);
        var batches = Regex.Split(
            scriptContent,
            @"^\s*GO\s*(?:--.*)?$",
            RegexOptions.Multiline | RegexOptions.IgnoreCase);

        foreach (var batch in batches)
        {
            var sql = batch.Trim();
            if (sql.Length == 0)
            {
                continue;
            }

            await connection.ExecuteAsync(sql);
        }

        logger.LogInformation("Database bootstrap script executed successfully.");
    }

    private static async Task ApplySchemaUpdatesAsync(
        DbConnectionFactory connectionFactory,
        ILogger<DbInitializer> logger)
    {
        using var connection = connectionFactory.CreateConnection();
        logger.LogInformation("Checking database schema updates...");

        const string checkColumnSql = @"
            IF OBJECT_ID('dbo.PeriodosAcademicos', 'U') IS NOT NULL
               AND NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.PeriodosAcademicos')
                      AND name = 'FechaCreacion'
               )
            BEGIN
                ALTER TABLE PeriodosAcademicos ADD FechaCreacion DATETIME DEFAULT GETDATE();
            END";
        await connection.ExecuteAsync(checkColumnSql);

        const string checkTwoFactorEnabledSql = @"
            IF OBJECT_ID('dbo.Usuarios', 'U') IS NOT NULL
               AND NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.Usuarios')
                      AND name = 'TwoFactorEnabled'
               )
            BEGIN
                ALTER TABLE Usuarios ADD TwoFactorEnabled BIT NOT NULL DEFAULT 0;
            END";
        await connection.ExecuteAsync(checkTwoFactorEnabledSql);

        const string checkTwoFactorSecretSql = @"
            IF OBJECT_ID('dbo.Usuarios', 'U') IS NOT NULL
               AND NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID('dbo.Usuarios')
                      AND name = 'TwoFactorSecret'
               )
            BEGIN
                ALTER TABLE Usuarios ADD TwoFactorSecret NVARCHAR(256) NULL;
            END";
        await connection.ExecuteAsync(checkTwoFactorSecretSql);

        const string createMatriculasCursosSql = @"
            IF OBJECT_ID('dbo.MatriculasCursos', 'U') IS NULL
            BEGIN
                CREATE TABLE MatriculasCursos (
                    Id INT PRIMARY KEY IDENTITY(1, 1),
                    AlumnoId INT NOT NULL,
                    CursoId INT NOT NULL,
                    FechaMatricula DATETIME NOT NULL DEFAULT GETDATE(),
                    Activo BIT NOT NULL DEFAULT 1,
                    CONSTRAINT UQ_MatriculasCursos_AlumnoCurso UNIQUE (AlumnoId, CursoId),
                    CONSTRAINT FK_MatriculasCursos_Alumnos FOREIGN KEY (AlumnoId) REFERENCES Alumnos(Id),
                    CONSTRAINT FK_MatriculasCursos_Cursos FOREIGN KEY (CursoId) REFERENCES Cursos(Id)
                );
            END";
        await connection.ExecuteAsync(createMatriculasCursosSql);

        const string createDocentesCursosSql = @"
            IF OBJECT_ID('dbo.DocentesCursos', 'U') IS NULL
            BEGIN
                CREATE TABLE DocentesCursos (
                    Id INT PRIMARY KEY IDENTITY(1, 1),
                    DocenteId INT NOT NULL,
                    CursoId INT NOT NULL,
                    FechaAsignacion DATETIME NOT NULL DEFAULT GETDATE(),
                    Activo BIT NOT NULL DEFAULT 1,
                    CONSTRAINT UQ_DocentesCursos_DocenteCurso UNIQUE (DocenteId, CursoId),
                    CONSTRAINT FK_DocentesCursos_Docentes FOREIGN KEY (DocenteId) REFERENCES Docentes(Id),
                    CONSTRAINT FK_DocentesCursos_Cursos FOREIGN KEY (CursoId) REFERENCES Cursos(Id)
                );
            END";
        await connection.ExecuteAsync(createDocentesCursosSql);

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
                IF OBJECT_ID('dbo.Planificaciones', 'U') IS NOT NULL
                   AND NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID('dbo.Planificaciones')
                          AND name = '{colName}'
                   )
                BEGIN
                    ALTER TABLE Planificaciones ADD {colDef};
                END";
            await connection.ExecuteAsync(addColSql);
        }

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
                IF OBJECT_ID('dbo.PlanificacionesMensuales', 'U') IS NOT NULL
                   AND NOT EXISTS (
                        SELECT 1 FROM sys.columns
                        WHERE object_id = OBJECT_ID('dbo.PlanificacionesMensuales')
                          AND name = '{colName}'
                   )
                BEGIN
                    ALTER TABLE PlanificacionesMensuales ADD {colDef};
                END";
            await connection.ExecuteAsync(addColSql);
        }
    }

    private static string? ResolveBootstrapScriptPath(string contentRootPath)
    {
        var localPath = Path.Combine(contentRootPath, "database", "db.sql");
        if (File.Exists(localPath))
        {
            return localPath;
        }

        var repositoryPath = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "database", "db.sql"));
        return File.Exists(repositoryPath) ? repositoryPath : null;
    }
}
