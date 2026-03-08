using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperTareaRepository : GenericDapperRepository<Tarea>, ITareaRepository
{
    protected override string TableName => "Tareas";

    public DapperTareaRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<Tarea>> GetByDocenteIdAsync(int docenteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tareas WHERE DocenteId = @DocenteId ORDER BY FechaEntrega DESC";
        return await connection.QueryAsync<Tarea>(sql, new { DocenteId = docenteId });
    }

    public async Task<IEnumerable<Tarea>> GetByCursoIdAsync(int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tareas WHERE CursoId = @CursoId AND Activa = 1 ORDER BY FechaEntrega DESC";
        return await connection.QueryAsync<Tarea>(sql, new { CursoId = cursoId });
    }

    public async Task<IEnumerable<Tarea>> GetByPeriodoIdAsync(int periodoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Tareas WHERE PeriodoAcademicoId = @PeriodoAcademicoId ORDER BY FechaEntrega DESC";
        return await connection.QueryAsync<Tarea>(sql, new { PeriodoAcademicoId = periodoId });
    }

    public async Task<IEnumerable<Tarea>> GetByAlumnoIdAsync(int alumnold, int periodoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Prioriza cursos matriculados explícitamente. Si el alumno no tiene matrículas,
        // usa el esquema anterior por horario/sección como fallback.
        const string sql = @"
            SELECT DISTINCT t.*
            FROM Tareas t
            WHERE t.PeriodoAcademicoId = @PeriodoId
              AND t.Activa = 1
              AND
              (
                EXISTS (
                    SELECT 1
                    FROM MatriculasCursos mc
                    WHERE mc.AlumnoId = @AlumnoId
                      AND mc.CursoId = t.CursoId
                      AND mc.Activo = 1
                )
                OR
                (
                    NOT EXISTS (
                        SELECT 1
                        FROM MatriculasCursos mc2
                        WHERE mc2.AlumnoId = @AlumnoId
                          AND mc2.Activo = 1
                    )
                    AND EXISTS (
                        SELECT 1
                        FROM Horarios h
                        INNER JOIN Alumnos a ON a.SeccionId = h.SeccionId
                        WHERE a.Id = @AlumnoId
                          AND h.CursoId = t.CursoId
                          AND h.PeriodoAcademicoId = t.PeriodoAcademicoId
                    )
                )
              )
            ORDER BY t.FechaEntrega DESC";
        return await connection.QueryAsync<Tarea>(sql, new { AlumnoId = alumnold, PeriodoId = periodoId });
    }
}
