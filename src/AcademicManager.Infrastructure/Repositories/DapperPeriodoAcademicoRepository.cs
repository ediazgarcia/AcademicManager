using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for PeriodoAcademico entity
/// Inherits common CRUD operations from GenericDapperRepository
/// </summary>
public class DapperPeriodoAcademicoRepository : GenericDapperRepository<PeriodoAcademico>, IPeriodoAcademicoRepository
{
    protected override string TableName => "PeriodosAcademicos";

    public DapperPeriodoAcademicoRepository(DbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public override async Task<IEnumerable<PeriodoAcademico>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM PeriodosAcademicos ORDER BY FechaInicio DESC";
        return await connection.QueryAsync<PeriodoAcademico>(sql);
    }

    public async Task<PeriodoAcademico?> GetActivoAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT TOP 1 * FROM PeriodosAcademicos WHERE Activo = 1 ORDER BY FechaInicio DESC";
        return await connection.QueryFirstOrDefaultAsync<PeriodoAcademico>(sql);
    }
}

