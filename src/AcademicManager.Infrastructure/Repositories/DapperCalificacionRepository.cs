using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperCalificacionRepository : GenericDapperRepository<Calificacion>, ICalificacionRepository
{
    protected override string TableName => "Calificaciones";

    public DapperCalificacionRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<Calificacion>> GetByEvaluacionIdAsync(int evaluacionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT c.*, a.Id, a.Codigo, a.Nombres, a.Apellidos
            FROM Calificaciones c
            JOIN Alumnos a ON c.AlumnoId = a.Id
            WHERE c.EvaluacionId = @EvaluacionId";

        return await connection.QueryAsync<Calificacion, Alumno, Calificacion>(
            sql,
            (cal, alu) => { cal.Alumno = alu; return cal; },
            new { EvaluacionId = evaluacionId },
            splitOn: "Id");
    }

    public async Task<Calificacion?> GetByEvaluacionAndAlumnoAsync(int evaluacionId, int alumnoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Calificaciones WHERE EvaluacionId = @EvaluacionId AND AlumnoId = @AlumnoId";
        return await connection.QuerySingleOrDefaultAsync<Calificacion>(sql, new { EvaluacionId = evaluacionId, AlumnoId = alumnoId });
    }

    public async Task<int> AddAsync(Calificacion calificacion)
    {
        return await CreateAsync(calificacion);
    }

    protected override string BuildInsertQuery(Calificacion entity)
    {
        return @"
            INSERT INTO Calificaciones (EvaluacionId, AlumnoId, Nota, PuntosExtra, Observacion, FechaRegistro)
            VALUES (@EvaluacionId, @AlumnoId, @Nota, @PuntosExtra, @Observacion, @FechaRegistro);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(Calificacion entity)
    {
        return @"
            UPDATE Calificaciones SET
                Nota = @Nota, PuntosExtra = @PuntosExtra, Observacion = @Observacion
            WHERE Id = @Id";
    }
}
