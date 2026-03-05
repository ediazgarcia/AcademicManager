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
        // Devuelve solo las tareas activas del período que pertenecen a los cursos
        // asignados al alumno (a través del horario → sección).
        const string sql = @"
            SELECT DISTINCT t.*
            FROM Tareas t
            INNER JOIN Horarios h ON h.CursoId = t.CursoId AND h.PeriodoAcademicoId = t.PeriodoAcademicoId
            INNER JOIN Alumnos a ON a.SeccionId = h.SeccionId
            WHERE a.Id = @AlumnoId
              AND t.PeriodoAcademicoId = @PeriodoId
              AND t.Activa = 1
            ORDER BY t.FechaEntrega DESC";
        return await connection.QueryAsync<Tarea>(sql, new { AlumnoId = alumnold, PeriodoId = periodoId });
    }
}
