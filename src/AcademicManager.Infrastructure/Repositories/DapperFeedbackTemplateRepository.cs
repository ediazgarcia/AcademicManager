using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperFeedbackTemplateRepository : GenericDapperRepository<FeedbackTemplate>, IFeedbackTemplateRepository
{
    protected override string TableName => "FeedbackTemplates";

    public DapperFeedbackTemplateRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<FeedbackTemplate>> GetByDocenteIdAsync(int docenteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM FeedbackTemplates
            WHERE DocenteId = @DocenteId
            ORDER BY Orden ASC";
        return await connection.QueryAsync<FeedbackTemplate>(sql, new { DocenteId = docenteId });
    }

    public async Task<IEnumerable<FeedbackTemplate>> GetByMateriaAsync(string materia)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM FeedbackTemplates
            WHERE Materia = @Materia AND Activa = 1
            ORDER BY Orden ASC";
        return await connection.QueryAsync<FeedbackTemplate>(sql, new { Materia = materia });
    }

    protected override string BuildInsertQuery(FeedbackTemplate entity)
    {
        return @"
            INSERT INTO FeedbackTemplates (DocenteId, Titulo, Contenido, Materia, Orden, Activa, CreatedAt)
            VALUES (@DocenteId, @Titulo, @Contenido, @Materia, @Orden, @Activa, @CreatedAt);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(FeedbackTemplate entity)
    {
        return @"
            UPDATE FeedbackTemplates SET
                Titulo = @Titulo,
                Contenido = @Contenido,
                Materia = @Materia,
                Orden = @Orden,
                Activa = @Activa,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";
    }
}
