using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for Grado entity
/// Inherits common CRUD operations from GenericDapperRepository
/// </summary>
public class DapperGradoRepository : GenericDapperRepository<Grado>, IGradoRepository
{
    protected override string TableName => "Grados";

    public DapperGradoRepository(DbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    /// <summary>
    /// Override to add custom ordering
    /// </summary>
    public override async Task<IEnumerable<Grado>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Grados ORDER BY Orden";
        return await connection.QueryAsync<Grado>(sql);
    }
}

