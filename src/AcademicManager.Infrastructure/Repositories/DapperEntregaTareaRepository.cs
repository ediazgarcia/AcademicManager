using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperEntregaTareaRepository : GenericDapperRepository<EntregaTarea>, IEntregaTareaRepository
{
    protected override string TableName => "EntregasTareas";

    public DapperEntregaTareaRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<EntregaTarea?> GetByTareaAndAlumnoAsync(int tareaId, int alumnold)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM EntregasTareas WHERE TareaId = @TareaId AND AlumnoId = @AlumnoId";
        return await connection.QueryFirstOrDefaultAsync<EntregaTarea>(sql, new { TareaId = tareaId, AlumnoId = alumnold });
    }

    public async Task<IEnumerable<EntregaTarea>> GetByTareaIdAsync(int tareaId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM EntregasTareas WHERE TareaId = @TareaId ORDER BY FechaEntrega DESC";
        return await connection.QueryAsync<EntregaTarea>(sql, new { TareaId = tareaId });
    }

    public async Task<IEnumerable<EntregaTarea>> GetByAlumnoIdAsync(int alumnold)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM EntregasTareas WHERE AlumnoId = @AlumnoId ORDER BY FechaEntrega DESC";
        return await connection.QueryAsync<EntregaTarea>(sql, new { AlumnoId = alumnold });
    }

    public async Task<int> GetTotalPuntosByAlumnoAndPeriodoAsync(int alumnold, int periodoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT ISNULL(SUM(e.Puntos), 0)
            FROM EntregasTareas e
            INNER JOIN Tareas t ON e.TareaId = t.Id
            WHERE e.AlumnoId = @AlumnoId AND t.PeriodoAcademicoId = @PeriodoId AND e.Puntos IS NOT NULL";
        return await connection.QuerySingleAsync<int>(sql, new { AlumnoId = alumnold, PeriodoId = periodoId });
    }

    public async Task<int> GetTotalPuntosMaximosByAlumnoAndPeriodoAsync(int alumnold, int periodoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT ISNULL(SUM(t.PuntosMaximos), 0)
            FROM Tareas t
            WHERE t.PeriodoAcademicoId = @PeriodoId AND t.Activa = 1";
        return await connection.QuerySingleAsync<int>(sql, new { PeriodoId = periodoId });
    }
}
