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

    public async Task<bool> AprobarAsync(int id, int aprobadorId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Planificaciones 
            SET Estado = @Estado, UsuarioAprobadorId = @UsuarioAprobadorId, 
                FechaAprobacion = @FechaAprobacion, FechaActualizacion = @FechaActualizacion
            WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Estado = "Aprobado",
            UsuarioAprobadorId = aprobadorId,
            FechaAprobacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        });
        return rows > 0;
    }

    public async Task<bool> RechazarAsync(int id, int rechazadorId, string motivo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE Planificaciones 
            SET Estado = @Estado, MotivoRechazo = @MotivoRechazo, FechaActualizacion = @FechaActualizacion
            WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            Estado = "Rechazado",
            MotivoRechazo = motivo,
            FechaActualizacion = DateTime.UtcNow
        });
        return rows > 0;
    }

    public async Task<IEnumerable<Planificacion>> GetByEstadoAsync(string estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Planificaciones WHERE Estado = @Estado ORDER BY FechaCreacion DESC";
        return await connection.QueryAsync<Planificacion>(sql, new { Estado = estado });
    }

    public async Task<IEnumerable<Planificacion>> BuscarAsync(string criterio)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM Planificaciones 
            WHERE Titulo LIKE @Criterio OR Descripcion LIKE @Criterio OR TituloUnidad LIKE @Criterio
            ORDER BY FechaCreacion DESC";
        return await connection.QueryAsync<Planificacion>(sql, new { Criterio = $"%{criterio}%" });
    }

    public async Task<IEnumerable<Planificacion>> ObtenerPaginadoAsync(int pagina, int tamanoPagina)
    {
        using var connection = _connectionFactory.CreateConnection();
        int offset = (pagina - 1) * tamanoPagina;
        const string sql = @"
            SELECT * FROM Planificaciones 
            ORDER BY FechaCreacion DESC
            OFFSET @Offset ROWS FETCH NEXT @TamanoPagina ROWS ONLY";
        return await connection.QueryAsync<Planificacion>(sql, new { Offset = offset, TamanoPagina = tamanoPagina });
    }

    public async Task<IEnumerable<Planificacion>> ObtenerPendientesAprobacionAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Planificaciones WHERE Estado = 'Enviado' ORDER BY FechaCreacion ASC";
        return await connection.QueryAsync<Planificacion>(sql);
    }

    public async Task<bool> RegistrarAuditoriaAsync(PlanificacionAuditoria auditoria)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO PlanificacionesAuditoria 
            (PlanificacionId, UsuarioId, Accion, EstadoAnterior, EstadoNuevo, 
             CamposModificados, Observaciones, FechaAccion, DireccionIP)
            VALUES 
            (@PlanificacionId, @UsuarioId, @Accion, @EstadoAnterior, @EstadoNuevo,
             @CamposModificados, @Observaciones, @FechaAccion, @DireccionIP)";
        var rows = await connection.ExecuteAsync(sql, auditoria);
        return rows > 0;
    }

    public async Task<IEnumerable<PlanificacionAuditoria>> ObtenerHistorialAsync(int planificacionId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM PlanificacionesAuditoria 
            WHERE PlanificacionId = @PlanificacionId
            ORDER BY FechaAccion DESC";
        return await connection.QueryAsync<PlanificacionAuditoria>(sql, new { PlanificacionId = planificacionId });
    }

    protected override string BuildInsertQuery(Planificacion entity)
    {
        return @"
            INSERT INTO Planificaciones (DocenteId, CursoId, PeriodoAcademicoId, SeccionId,
                Titulo, Descripcion, Objetivos, Contenido, Metodologia, RecursosDidacticos, Recursos,
                Evaluacion, Observaciones, FechaClase, Estado, FechaCreacion, FechaActualizacion,
                AnoAcademico, TipoplanificacionAcademico,
                Mes, TituloUnidad, SituacionAprendizaje,
                CompetenciasFundamentales, CompetenciasEspecificas,
                ContenidosConceptuales, ContenidosProcedimentales, ContenidosActitudinales,
                IndicadoresLogro, EstrategiasEnsenanza, ActividadesEvaluacion, EjesTransversales,
                CriteriosEvaluacion, UsuarioAprobadorId, FechaAprobacion, MotivoRechazo)
            VALUES (@DocenteId, @CursoId, @PeriodoAcademicoId, @SeccionId,
                @Titulo, @Descripcion, @Objetivos, @Contenido, @Metodologia, @RecursosDidacticos, @Recursos,
                @Evaluacion, @Observaciones, @FechaClase, @Estado, @FechaCreacion, @FechaActualizacion,
                @AnoAcademico, @TipoplanificacionAcademico,
                @Mes, @TituloUnidad, @SituacionAprendizaje,
                @CompetenciasFundamentales, @CompetenciasEspecificas,
                @ContenidosConceptuales, @ContenidosProcedimentales, @ContenidosActitudinales,
                @IndicadoresLogro, @EstrategiasEnsenanza, @ActividadesEvaluacion, @EjesTransversales,
                @CriteriosEvaluacion, @UsuarioAprobadorId, @FechaAprobacion, @MotivoRechazo);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(Planificacion entity)
    {
        return @"
            UPDATE Planificaciones SET
                DocenteId = @DocenteId, CursoId = @CursoId, PeriodoAcademicoId = @PeriodoAcademicoId,
                SeccionId = @SeccionId, Titulo = @Titulo, Descripcion = @Descripcion,
                Objetivos = @Objetivos, Contenido = @Contenido, Metodologia = @Metodologia,
                RecursosDidacticos = @RecursosDidacticos, Recursos = @Recursos,
                Evaluacion = @Evaluacion, Observaciones = @Observaciones,
                FechaClase = @FechaClase, Estado = @Estado, FechaActualizacion = @FechaActualizacion,
                AnoAcademico = @AnoAcademico, TipoplanificacionAcademico = @TipoplanificacionAcademico,
                Mes = @Mes, TituloUnidad = @TituloUnidad, SituacionAprendizaje = @SituacionAprendizaje,
                CompetenciasFundamentales = @CompetenciasFundamentales,
                CompetenciasEspecificas = @CompetenciasEspecificas,
                ContenidosConceptuales = @ContenidosConceptuales,
                ContenidosProcedimentales = @ContenidosProcedimentales,
                ContenidosActitudinales = @ContenidosActitudinales,
                IndicadoresLogro = @IndicadoresLogro,
                EstrategiasEnsenanza = @EstrategiasEnsenanza,
                ActividadesEvaluacion = @ActividadesEvaluacion,
                EjesTransversales = @EjesTransversales,
                CriteriosEvaluacion = @CriteriosEvaluacion,
                UsuarioAprobadorId = @UsuarioAprobadorId,
                FechaAprobacion = @FechaAprobacion,
                MotivoRechazo = @MotivoRechazo,
                FechaInicio = @FechaInicio,
                FechaFin = @FechaFin
            WHERE Id = @Id";
    }
}
