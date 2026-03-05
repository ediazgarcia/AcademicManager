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
        const string sql = "SELECT * FROM Cursos WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Curso>(sql, new { Id = id });
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
        const string sql = "SELECT * FROM Cursos ORDER BY GradoId, Nombre";
        return await connection.QueryAsync<Curso>(sql);
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

