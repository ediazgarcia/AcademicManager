using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperAlumnoRepository : IAlumnoRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public DapperAlumnoRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Alumno?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Alumnos WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Alumno>(sql, new { Id = id });
    }

    public async Task<Alumno?> GetByCodigoAsync(string codigo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Alumnos WHERE Codigo = @Codigo";
        return await connection.QueryFirstOrDefaultAsync<Alumno>(sql, new { Codigo = codigo });
    }

    public async Task<IEnumerable<Alumno>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.*, g.Nombre as GradoNombre,
                   s.Nombre as SeccionNombre
            FROM Alumnos a
            LEFT JOIN Grados g ON a.GradoId = g.Id
            LEFT JOIN Secciones s ON a.SeccionId = s.Id
            ORDER BY a.Apellidos, a.Nombres";
        return await connection.QueryAsync<Alumno>(sql);
    }

    public async Task<IEnumerable<Alumno>> GetByGradoIdAsync(int gradoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Alumnos WHERE GradoId = @GradoId ORDER BY Apellidos, Nombres";
        return await connection.QueryAsync<Alumno>(sql, new { GradoId = gradoId });
    }

    public async Task<IEnumerable<Alumno>> GetBySeccionIdAsync(int seccionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Alumnos WHERE SeccionId = @SeccionId ORDER BY Apellidos, Nombres";
        return await connection.QueryAsync<Alumno>(sql, new { SeccionId = seccionId });
    }

    public async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Alumnos WHERE Activo = 1";
        return await connection.QuerySingleAsync<int>(sql);
    }

    public async Task<int> CreateAsync(Alumno alumno)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO Alumnos (Codigo, Nombres, Apellidos, FechaNacimiento, Genero, Direccion,
                Telefono, Email, NombreApoderado, TelefonoApoderado, GradoId, SeccionId, Activo, FechaRegistro)
            VALUES (@Codigo, @Nombres, @Apellidos, @FechaNacimiento, @Genero, @Direccion,
                @Telefono, @Email, @NombreApoderado, @TelefonoApoderado, @GradoId, @SeccionId, @Activo, @FechaRegistro);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return await connection.QuerySingleAsync<int>(sql, alumno);
    }

    public async Task<bool> UpdateAsync(Alumno alumno)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Alumnos SET
                Codigo = @Codigo, Nombres = @Nombres, Apellidos = @Apellidos,
                FechaNacimiento = @FechaNacimiento, Genero = @Genero, Direccion = @Direccion,
                Telefono = @Telefono, Email = @Email, NombreApoderado = @NombreApoderado,
                TelefonoApoderado = @TelefonoApoderado, GradoId = @GradoId,
                SeccionId = @SeccionId, Activo = @Activo
            WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, alumno);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM Alumnos WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}
