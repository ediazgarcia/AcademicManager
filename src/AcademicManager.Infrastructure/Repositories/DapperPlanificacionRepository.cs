using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperPlanificacionRepository : GenericDapperRepository<Planificacion>, IPlanificacionRepository
{
    protected override string TableName => "Planificaciones";

    public DapperPlanificacionRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public override async Task<Planificacion?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT pl.*, d.Id, d.Nombres, d.Apellidos, c.Id, c.Codigo, c.Nombre,
                   p.Id, p.Nombre, s.Id, s.Nombre
            FROM Planificaciones pl
            LEFT JOIN Docentes d ON pl.DocenteId = d.Id
            LEFT JOIN Cursos c ON pl.CursoId = c.Id
            LEFT JOIN PeriodosAcademicos p ON pl.PeriodoAcademicoId = p.Id
            LEFT JOIN Secciones s ON pl.SeccionId = s.Id
            WHERE pl.Id = @Id";
        var result = await connection.QueryAsync<Planificacion, Docente, Curso, PeriodoAcademico, Seccion, Planificacion>(
            sql,
            (plan, docente, curso, periodo, seccion) =>
            {
                plan.Docente = docente;
                plan.Curso = curso;
                plan.PeriodoAcademico = periodo;
                plan.Seccion = seccion;
                return plan;
            },
            new { Id = id },
            splitOn: "Id,Id,Id,Id");
        return result.FirstOrDefault();
    }

    public override async Task<IEnumerable<Planificacion>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT pl.*, d.Id, d.Nombres, d.Apellidos, c.Id, c.Codigo, c.Nombre,
                   p.Id, p.Nombre, s.Id, s.Nombre
            FROM Planificaciones pl
            LEFT JOIN Docentes d ON pl.DocenteId = d.Id
            LEFT JOIN Cursos c ON pl.CursoId = c.Id
            LEFT JOIN PeriodosAcademicos p ON pl.PeriodoAcademicoId = p.Id
            LEFT JOIN Secciones s ON pl.SeccionId = s.Id
            ORDER BY pl.FechaClase DESC";
        return await connection.QueryAsync<Planificacion, Docente, Curso, PeriodoAcademico, Seccion, Planificacion>(
            sql,
            (plan, docente, curso, periodo, seccion) =>
            {
                plan.Docente = docente;
                plan.Curso = curso;
                plan.PeriodoAcademico = periodo;
                plan.Seccion = seccion;
                return plan;
            },
            splitOn: "Id,Id,Id,Id");
    }

    public async Task<IEnumerable<Planificacion>> GetByDocenteIdAsync(int docenteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT pl.*, c.Id, c.Codigo, c.Nombre, s.Id, s.Nombre
            FROM Planificaciones pl
            LEFT JOIN Cursos c ON pl.CursoId = c.Id
            LEFT JOIN Secciones s ON pl.SeccionId = s.Id
            WHERE pl.DocenteId = @DocenteId
            ORDER BY pl.FechaClase DESC";
        return await connection.QueryAsync<Planificacion, Curso, Seccion, Planificacion>(
            sql,
            (plan, curso, seccion) =>
            {
                plan.Curso = curso;
                plan.Seccion = seccion;
                return plan;
            },
            new { DocenteId = docenteId },
            splitOn: "Id,Id");
    }

    public async Task<IEnumerable<Planificacion>> GetByCursoIdAsync(int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Planificaciones WHERE CursoId = @CursoId ORDER BY FechaClase DESC";
        return await connection.QueryAsync<Planificacion>(sql, new { CursoId = cursoId });
    }

    public async Task<IEnumerable<Planificacion>> GetByPeriodoIdAsync(int periodoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Planificaciones WHERE PeriodoAcademicoId = @PeriodoId ORDER BY FechaClase DESC";
        return await connection.QueryAsync<Planificacion>(sql, new { PeriodoId = periodoId });
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Planificaciones SET Estado = @Estado, FechaActualizacion = @FechaActualizacion
            WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, Estado = estado, FechaActualizacion = DateTime.UtcNow });
        return rows > 0;
    }

    protected override string BuildInsertQuery(Planificacion entity)
    {
        return @"
            INSERT INTO Planificaciones (DocenteId, CursoId, PeriodoAcademicoId, SeccionId,
                Titulo, Descripcion, Objetivos, Contenido, Metodologia, RecursosDidacticos,
                Evaluacion, FechaClase, Estado, FechaCreacion)
            VALUES (@DocenteId, @CursoId, @PeriodoAcademicoId, @SeccionId,
                @Titulo, @Descripcion, @Objetivos, @Contenido, @Metodologia, @RecursosDidacticos,
                @Evaluacion, @FechaClase, @Estado, @FechaCreacion);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(Planificacion entity)
    {
        return @"
            UPDATE Planificaciones SET
                DocenteId = @DocenteId, CursoId = @CursoId, PeriodoAcademicoId = @PeriodoAcademicoId,
                SeccionId = @SeccionId, Titulo = @Titulo, Descripcion = @Descripcion,
                Objetivos = @Objetivos, Contenido = @Contenido, Metodologia = @Metodologia,
                RecursosDidacticos = @RecursosDidacticos, Evaluacion = @Evaluacion,
                FechaClase = @FechaClase, Estado = @Estado, FechaActualizacion = @FechaActualizacion
            WHERE Id = @Id";
    }
}
