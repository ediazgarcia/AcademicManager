using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperStudentNotificationRepository : GenericDapperRepository<StudentNotification>, IStudentNotificationRepository
{
    protected override string TableName => "StudentNotifications";

    public DapperStudentNotificationRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<StudentNotification>> GetByAlumnoIdAsync(int alumnoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM StudentNotifications
            WHERE AlumnoId = @AlumnoId
            ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<StudentNotification>(sql, new { AlumnoId = alumnoId });
    }

    public async Task<IEnumerable<StudentNotification>> GetUnreadByAlumnoIdAsync(int alumnoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM StudentNotifications
            WHERE AlumnoId = @AlumnoId AND Leida = 0
            ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<StudentNotification>(sql, new { AlumnoId = alumnoId });
    }

    protected override string BuildInsertQuery(StudentNotification entity)
    {
        return @"
            INSERT INTO StudentNotifications (AlumnoId, Tipo, Titulo, Contenido, Leida, CreatedAt, LeidaAt, RelatedEntityId)
            VALUES (@AlumnoId, @Tipo, @Titulo, @Contenido, @Leida, @CreatedAt, @LeidaAt, @RelatedEntityId);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(StudentNotification entity)
    {
        return @"
            UPDATE StudentNotifications SET
                Leida = @Leida,
                LeidaAt = @LeidaAt
            WHERE Id = @Id";
    }
}
