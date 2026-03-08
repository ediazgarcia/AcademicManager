using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperReportTemplateRepository : GenericDapperRepository<ReportTemplate>, IReportTemplateRepository
{
    protected override string TableName => "ReportTemplates";

    public DapperReportTemplateRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<ReportTemplate>> GetByCoordinadorIdAsync(int coordinadorId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM ReportTemplates
            WHERE CoordinadorId = @CoordinadorId AND Activa = 1
            ORDER BY Orden ASC";
        return await connection.QueryAsync<ReportTemplate>(sql, new { CoordinadorId = coordinadorId });
    }

    public async Task<IEnumerable<ReportTemplate>> GetByTipoAsync(string tipoReporte)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM ReportTemplates
            WHERE TipoReporte = @TipoReporte AND Activa = 1
            ORDER BY Orden ASC";
        return await connection.QueryAsync<ReportTemplate>(sql, new { TipoReporte = tipoReporte });
    }

    protected override string BuildInsertQuery(ReportTemplate entity)
    {
        return @"
            INSERT INTO ReportTemplates (CoordinadorId, Nombre, TipoReporte, Filtros, Activa, Orden, CreatedAt)
            VALUES (@CoordinadorId, @Nombre, @TipoReporte, @Filtros, @Activa, @Orden, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(ReportTemplate entity)
    {
        return @"
            UPDATE ReportTemplates SET
                Nombre = @Nombre,
                TipoReporte = @TipoReporte,
                Filtros = @Filtros,
                Activa = @Activa,
                Orden = @Orden,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
    }
}
