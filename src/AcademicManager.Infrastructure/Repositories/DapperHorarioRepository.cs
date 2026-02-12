using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperHorarioRepository : GenericDapperRepository<Horario>, IHorarioRepository
{
    protected override string TableName => "Horarios";

    public DapperHorarioRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public override async Task<Horario?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT h.*, c.Id, c.Codigo, c.Nombre, d.Id, d.Nombres, d.Apellidos,
                   s.Id, s.Nombre, p.Id, p.Nombre
            FROM Horarios h
            LEFT JOIN Cursos c ON h.CursoId = c.Id
            LEFT JOIN Docentes d ON h.DocenteId = d.Id
            LEFT JOIN Secciones s ON h.SeccionId = s.Id
            LEFT JOIN PeriodosAcademicos p ON h.PeriodoAcademicoId = p.Id
            WHERE h.Id = @Id";
        var result = await connection.QueryAsync<Horario, Curso, Docente, Seccion, PeriodoAcademico, Horario>(
            sql,
            (horario, curso, docente, seccion, periodo) =>
            {
                horario.Curso = curso;
                horario.Docente = docente;
                horario.Seccion = seccion;
                horario.PeriodoAcademico = periodo;
                return horario;
            },
            new { Id = id },
            splitOn: "Id,Id,Id,Id");
        return result.FirstOrDefault();
    }

    public override async Task<IEnumerable<Horario>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT h.*, c.Id, c.Codigo, c.Nombre, d.Id, d.Nombres, d.Apellidos,
                   s.Id, s.Nombre, p.Id, p.Nombre
            FROM Horarios h
            LEFT JOIN Cursos c ON h.CursoId = c.Id
            LEFT JOIN Docentes d ON h.DocenteId = d.Id
            LEFT JOIN Secciones s ON h.SeccionId = s.Id
            LEFT JOIN PeriodosAcademicos p ON h.PeriodoAcademicoId = p.Id
            ORDER BY h.DiaSemana, h.HoraInicio";
        return await connection.QueryAsync<Horario, Curso, Docente, Seccion, PeriodoAcademico, Horario>(
            sql,
            (horario, curso, docente, seccion, periodo) =>
            {
                horario.Curso = curso;
                horario.Docente = docente;
                horario.Seccion = seccion;
                horario.PeriodoAcademico = periodo;
                return horario;
            },
            splitOn: "Id,Id,Id,Id");
    }

    public async Task<IEnumerable<Horario>> GetByDocenteIdAsync(int docenteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT h.*, c.Id, c.Codigo, c.Nombre, s.Id, s.Nombre
            FROM Horarios h
            LEFT JOIN Cursos c ON h.CursoId = c.Id
            LEFT JOIN Secciones s ON h.SeccionId = s.Id
            WHERE h.DocenteId = @DocenteId
            ORDER BY h.DiaSemana, h.HoraInicio";
        return await connection.QueryAsync<Horario, Curso, Seccion, Horario>(
            sql,
            (horario, curso, seccion) =>
            {
                horario.Curso = curso;
                horario.Seccion = seccion;
                return horario;
            },
            new { DocenteId = docenteId },
            splitOn: "Id,Id");
    }

    public async Task<IEnumerable<Horario>> GetBySeccionIdAsync(int seccionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT h.*, c.Id, c.Codigo, c.Nombre, d.Id, d.Nombres, d.Apellidos
            FROM Horarios h
            LEFT JOIN Cursos c ON h.CursoId = c.Id
            LEFT JOIN Docentes d ON h.DocenteId = d.Id
            WHERE h.SeccionId = @SeccionId
            ORDER BY h.DiaSemana, h.HoraInicio";
        return await connection.QueryAsync<Horario, Curso, Docente, Horario>(
            sql,
            (horario, curso, docente) =>
            {
                horario.Curso = curso;
                horario.Docente = docente;
                return horario;
            },
            new { SeccionId = seccionId },
            splitOn: "Id,Id");
    }

    public async Task<IEnumerable<Horario>> GetByPeriodoIdAsync(int periodoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Horarios WHERE PeriodoAcademicoId = @PeriodoId ORDER BY DiaSemana, HoraInicio";
        return await connection.QueryAsync<Horario>(sql, new { PeriodoId = periodoId });
    }

    protected override string BuildInsertQuery(Horario entity)
    {
        return @"
            INSERT INTO Horarios (CursoId, DocenteId, SeccionId, PeriodoAcademicoId, DiaSemana,
                HoraInicio, HoraFin, Aula, Activo)
            VALUES (@CursoId, @DocenteId, @SeccionId, @PeriodoAcademicoId, @DiaSemana,
                @HoraInicio, @HoraFin, @Aula, @Activo);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(Horario entity)
    {
        return @"
            UPDATE Horarios SET
                CursoId = @CursoId, DocenteId = @DocenteId, SeccionId = @SeccionId,
                PeriodoAcademicoId = @PeriodoAcademicoId, DiaSemana = @DiaSemana,
                HoraInicio = @HoraInicio, HoraFin = @HoraFin, Aula = @Aula, Activo = @Activo
            WHERE Id = @Id";
    }
}
