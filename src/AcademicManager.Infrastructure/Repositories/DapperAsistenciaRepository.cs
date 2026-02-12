using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperAsistenciaRepository : GenericDapperRepository<Asistencia>, IAsistenciaRepository
{
    protected override string TableName => "Asistencias";

    public DapperAsistenciaRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<Asistencia>> GetByPlanificacionIdAndDateAsync(int planificacionId, DateTime fecha)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT a.*, al.Id, al.Codigo, al.Nombres, al.Apellidos
            FROM Asistencias a
            JOIN Alumnos al ON a.AlumnoId = al.Id
            WHERE a.PlanificacionId = @PlanificacionId AND CAST(a.Fecha AS DATE) = CAST(@Fecha AS DATE)";

        return await connection.QueryAsync<Asistencia, Alumno, Asistencia>(
            sql,
            (asis, alu) => { asis.Alumno = alu; return asis; },
            new { PlanificacionId = planificacionId, Fecha = fecha },
            splitOn: "Id");
    }

    public async Task<IEnumerable<Asistencia>> GetByPlanificacionIdAsync(int planificacionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Asistencias WHERE PlanificacionId = @PlanificacionId ORDER BY Fecha DESC";
        return await connection.QueryAsync<Asistencia>(sql, new { PlanificacionId = planificacionId });
    }

    public async Task<int> AddAsync(Asistencia asistencia)
    {
        return await CreateAsync(asistencia);
    }

    protected override string BuildInsertQuery(Asistencia entity)
    {
        return @"
            INSERT INTO Asistencias (PlanificacionId, AlumnoId, Fecha, Estado, Observacion, FechaRegistro)
            VALUES (@PlanificacionId, @AlumnoId, @Fecha, @Estado, @Observacion, @FechaRegistro);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(Asistencia entity)
    {
        return @"
            UPDATE Asistencias SET
                Estado = @Estado, Observacion = @Observacion
            WHERE Id = @Id";
    }
}
