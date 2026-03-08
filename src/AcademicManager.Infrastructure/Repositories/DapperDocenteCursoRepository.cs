using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperDocenteCursoRepository : IDocenteCursoRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public DapperDocenteCursoRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<DocenteCurso?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM DocentesCursos WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<DocenteCurso>(sql, new { Id = id });
    }

    public async Task<IEnumerable<DocenteCurso>> GetByDocenteIdAsync(int docenteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT *
            FROM DocentesCursos
            WHERE DocenteId = @DocenteId AND Activo = 1
            ORDER BY FechaAsignacion DESC";
        return await connection.QueryAsync<DocenteCurso>(sql, new { DocenteId = docenteId });
    }

    public async Task<IEnumerable<DocenteCurso>> GetByCursoIdAsync(int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT *
            FROM DocentesCursos
            WHERE CursoId = @CursoId AND Activo = 1
            ORDER BY FechaAsignacion DESC";
        return await connection.QueryAsync<DocenteCurso>(sql, new { CursoId = cursoId });
    }

    public async Task<bool> ExistsAsync(int docenteId, int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(1)
            FROM DocentesCursos
            WHERE DocenteId = @DocenteId AND CursoId = @CursoId AND Activo = 1";
        var count = await connection.QuerySingleAsync<int>(sql, new { DocenteId = docenteId, CursoId = cursoId });
        return count > 0;
    }

    public async Task<int> CreateAsync(DocenteCurso asignacion)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO DocentesCursos (DocenteId, CursoId, FechaAsignacion, Activo)
            VALUES (@DocenteId, @CursoId, @FechaAsignacion, @Activo);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return await connection.QuerySingleAsync<int>(sql, asignacion);
    }

    public async Task<bool> DeleteByDocenteAndCursoAsync(int docenteId, int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM DocentesCursos
            WHERE DocenteId = @DocenteId AND CursoId = @CursoId";
        var rows = await connection.ExecuteAsync(sql, new { DocenteId = docenteId, CursoId = cursoId });
        return rows > 0;
    }

    public async Task<int> DeleteByDocenteAsync(int docenteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM DocentesCursos
            WHERE DocenteId = @DocenteId";
        return await connection.ExecuteAsync(sql, new { DocenteId = docenteId });
    }
}
