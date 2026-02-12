using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperEvaluacionRepository : GenericDapperRepository<Evaluacion>, IEvaluacionRepository
{
    protected override string TableName => "Evaluaciones";

    public DapperEvaluacionRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<Evaluacion>> GetByPlanificacionIdAsync(int planificacionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Evaluaciones WHERE PlanificacionId = @PlanificacionId";
        return await connection.QueryAsync<Evaluacion>(sql, new { PlanificacionId = planificacionId });
    }

    public async Task<int> AddAsync(Evaluacion evaluacion)
    {
        return await CreateAsync(evaluacion);
    }

    protected override string BuildInsertQuery(Evaluacion entity)
    {
        return @"
            INSERT INTO Evaluaciones (PlanificacionId, Nombre, Descripcion, Peso, MaximoPuntaje, FechaEvaluacion, Activo, FechaRegistro)
            VALUES (@PlanificacionId, @Nombre, @Descripcion, @Peso, @MaximoPuntaje, @FechaEvaluacion, @Activo, @FechaRegistro);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(Evaluacion entity)
    {
        return @"
            UPDATE Evaluaciones SET
                Nombre = @Nombre, Descripcion = @Descripcion, Peso = @Peso,
                MaximoPuntaje = @MaximoPuntaje, FechaEvaluacion = @FechaEvaluacion, Activo = @Activo
            WHERE Id = @Id";
    }
}
