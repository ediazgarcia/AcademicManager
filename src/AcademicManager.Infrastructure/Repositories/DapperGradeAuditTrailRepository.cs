using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperGradeAuditTrailRepository : GenericDapperRepository<GradeAuditTrail>, IGradeAuditTrailRepository
{
    protected override string TableName => "GradeAuditTrails";

    public DapperGradeAuditTrailRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<GradeAuditTrail>> GetByEntregaIdAsync(int entregaTareaId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM GradeAuditTrails
            WHERE EntregaTareaId = @EntregaTareaId
            ORDER BY Timestamp DESC";
        return await connection.QueryAsync<GradeAuditTrail>(sql, new { EntregaTareaId = entregaTareaId });
    }

    public async Task<IEnumerable<GradeAuditTrail>> GetByDocenteIdAsync(int docenteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM GradeAuditTrails
            WHERE DocenteId = @DocenteId
            ORDER BY Timestamp DESC";
        return await connection.QueryAsync<GradeAuditTrail>(sql, new { DocenteId = docenteId });
    }

    public async Task<IEnumerable<GradeAuditTrail>> GetByDateRangeAsync(DateTime desde, DateTime hasta)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM GradeAuditTrails
            WHERE Timestamp BETWEEN @Desde AND @Hasta
            ORDER BY Timestamp DESC";
        return await connection.QueryAsync<GradeAuditTrail>(sql, new { Desde = desde, Hasta = hasta });
    }

    protected override string BuildInsertQuery(GradeAuditTrail entity)
    {
        return @"
            INSERT INTO GradeAuditTrails (EntregaTareaId, DocenteId, NotaAnterior, NotaNueva, Razon, Timestamp)
            VALUES (@EntregaTareaId, @DocenteId, @NotaAnterior, @NotaNueva, @Razon, @Timestamp);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(GradeAuditTrail entity)
    {
        return @"
            UPDATE GradeAuditTrails SET
                Razon = @Razon
            WHERE Id = @Id";
    }
}
