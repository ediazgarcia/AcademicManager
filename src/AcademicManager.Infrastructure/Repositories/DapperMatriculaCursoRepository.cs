using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperMatriculaCursoRepository : IMatriculaCursoRepository
{
    private readonly DbConnectionFactory _connectionFactory;

    public DapperMatriculaCursoRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<MatriculaCurso?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM MatriculasCursos WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<MatriculaCurso>(sql, new { Id = id });
    }

    public async Task<IEnumerable<MatriculaCurso>> GetByAlumnoIdAsync(int alumnoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT *
            FROM MatriculasCursos
            WHERE AlumnoId = @AlumnoId AND Activo = 1
            ORDER BY FechaMatricula DESC";
        return await connection.QueryAsync<MatriculaCurso>(sql, new { AlumnoId = alumnoId });
    }

    public async Task<IEnumerable<MatriculaCurso>> GetByCursoIdAsync(int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT *
            FROM MatriculasCursos
            WHERE CursoId = @CursoId AND Activo = 1
            ORDER BY FechaMatricula DESC";
        return await connection.QueryAsync<MatriculaCurso>(sql, new { CursoId = cursoId });
    }

    public async Task<bool> ExistsAsync(int alumnoId, int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT COUNT(1)
            FROM MatriculasCursos
            WHERE AlumnoId = @AlumnoId AND CursoId = @CursoId AND Activo = 1";
        var count = await connection.QuerySingleAsync<int>(sql, new { AlumnoId = alumnoId, CursoId = cursoId });
        return count > 0;
    }

    public async Task<int> CreateAsync(MatriculaCurso matricula)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO MatriculasCursos (AlumnoId, CursoId, FechaMatricula, Activo)
            VALUES (@AlumnoId, @CursoId, @FechaMatricula, @Activo);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
        return await connection.QuerySingleAsync<int>(sql, matricula);
    }

    public async Task<bool> DeleteByAlumnoAndCursoAsync(int alumnoId, int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM MatriculasCursos
            WHERE AlumnoId = @AlumnoId AND CursoId = @CursoId";
        var rows = await connection.ExecuteAsync(sql, new { AlumnoId = alumnoId, CursoId = cursoId });
        return rows > 0;
    }

    public async Task<int> DeleteByAlumnoAsync(int alumnoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            DELETE FROM MatriculasCursos
            WHERE AlumnoId = @AlumnoId";
        return await connection.ExecuteAsync(sql, new { AlumnoId = alumnoId });
    }
}
