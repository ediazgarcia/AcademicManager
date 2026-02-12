using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for Curso entity
/// Inherits common CRUD operations from GenericDapperRepository
/// </summary>
public class DapperCursoRepository : GenericDapperRepository<Curso>, ICursoRepository
{
    protected override string TableName => "Cursos";

    public DapperCursoRepository(DbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public override async Task<Curso?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT c.*, g.Id, g.Nombre, g.Nivel, g.Orden, g.Activo
            FROM Cursos c
            LEFT JOIN Grados g ON c.GradoId = g.Id
            WHERE c.Id = @Id";
        var result = await connection.QueryAsync<Curso, Grado, Curso>(
            sql,
            (curso, grado) => { curso.Grado = grado; return curso; },
            new { Id = id },
            splitOn: "Id");
        return result.FirstOrDefault();
    }

    public async Task<Curso?> GetByCodigoAsync(string codigo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Cursos WHERE Codigo = @Codigo";
        return await connection.QueryFirstOrDefaultAsync<Curso>(sql, new { Codigo = codigo });
    }

    public override async Task<IEnumerable<Curso>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT c.*, g.Id, g.Nombre, g.Nivel, g.Orden, g.Activo
            FROM Cursos c
            LEFT JOIN Grados g ON c.GradoId = g.Id
            ORDER BY c.Nombre";
        return await connection.QueryAsync<Curso, Grado, Curso>(
            sql,
            (curso, grado) => { curso.Grado = grado; return curso; },
            splitOn: "Id");
    }

    public async Task<IEnumerable<Curso>> GetByGradoIdAsync(int gradoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Cursos WHERE GradoId = @GradoId ORDER BY Nombre";
        return await connection.QueryAsync<Curso>(sql, new { GradoId = gradoId });
    }

    public override async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Cursos WHERE Activo = 1";
        return await connection.QuerySingleAsync<int>(sql);
    }
}

