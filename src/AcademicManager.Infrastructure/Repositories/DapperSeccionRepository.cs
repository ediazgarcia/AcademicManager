using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for Seccion entity
/// Inherits common CRUD operations from GenericDapperRepository
/// </summary>
public class DapperSeccionRepository : GenericDapperRepository<Seccion>, ISeccionRepository
{
    protected override string TableName => "Secciones";

    public DapperSeccionRepository(DbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public override async Task<Seccion?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Secciones WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<Seccion>(sql, new { Id = id });
    }

    public override async Task<IEnumerable<Seccion>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Secciones ORDER BY GradoId, Nombre";
        return await connection.QueryAsync<Seccion>(sql);
    }

    public async Task<IEnumerable<Seccion>> GetByGradoIdAsync(int gradoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Secciones WHERE GradoId = @GradoId ORDER BY Nombre";
        return await connection.QueryAsync<Seccion>(sql, new { GradoId = gradoId });
    }
}

