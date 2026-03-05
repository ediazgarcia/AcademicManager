using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperPlanificacionMensualRepository : GenericDapperRepository<PlanificacionMensual>, IPlanificacionMensualRepository
{
    protected override string TableName => "PlanificacionesMensuales";

    public DapperPlanificacionMensualRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public override async Task<PlanificacionMensual?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT pm.*, d.Id, d.Nombres, d.Apellidos, c.Id, c.Codigo, c.Nombre,
                   s.Id, s.Nombre, p.Id, p.Nombre
            FROM PlanificacionesMensuales pm
            LEFT JOIN Docentes d ON pm.DocenteId = d.Id
            LEFT JOIN Cursos c ON pm.CursoId = c.Id
            LEFT JOIN Secciones s ON pm.SeccionId = s.Id
            LEFT JOIN PeriodosAcademicos p ON pm.PeriodoAcademicoId = p.Id
            WHERE pm.Id = @Id";
        var result = await connection.QueryAsync<PlanificacionMensual, Docente, Curso, Seccion, PeriodoAcademico, PlanificacionMensual>(
            sql,
            (plan, docente, curso, seccion, periodo) =>
            {
                plan.Docente = docente;
                plan.Curso = curso;
                plan.Seccion = seccion;
                plan.PeriodoAcademico = periodo;
                return plan;
            },
            new { Id = id },
            splitOn: "Id,Id,Id,Id");
        return result.FirstOrDefault();
    }

    public async Task<IEnumerable<PlanificacionMensual>> GetByDocenteIdAsync(int docenteId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT pm.*, c.Id, c.Nombre, s.Id, s.Nombre
            FROM PlanificacionesMensuales pm
            LEFT JOIN Cursos c ON pm.CursoId = c.Id
            LEFT JOIN Secciones s ON pm.SeccionId = s.Id
            WHERE pm.DocenteId = @DocenteId
            ORDER BY pm.FechaCreacion DESC";
        return await connection.QueryAsync<PlanificacionMensual, Curso, Seccion, PlanificacionMensual>(
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

    public async Task<IEnumerable<PlanificacionMensual>> GetByCursoIdAsync(int cursoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM PlanificacionesMensuales WHERE CursoId = @CursoId ORDER BY NumeroMes ASC";
        return await connection.QueryAsync<PlanificacionMensual>(sql, new { CursoId = cursoId });
    }

    public async Task<IEnumerable<PlanificacionMensual>> GetByPlanificacionAnualIdAsync(int planificacionAnualId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT pm.*
            FROM PlanificacionesMensuales pm
            WHERE pm.PlanificacionId = @PlanificacionId
            ORDER BY pm.NumeroMes ASC";
        return await connection.QueryAsync<PlanificacionMensual>(sql, new { PlanificacionId = planificacionAnualId });
    }

    public async Task<PlanificacionMensual?> GetByPlanificacionAnualIdAndMesAsync(int planificacionAnualId, int numeroMes)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT pm.*
            FROM PlanificacionesMensuales pm
            WHERE pm.PlanificacionId = @PlanificacionId AND pm.NumeroMes = @NumeroMes";
        return await connection.QueryFirstOrDefaultAsync<PlanificacionMensual>(
            sql, new { PlanificacionId = planificacionAnualId, NumeroMes = numeroMes });
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE PlanificacionesMensuales SET Estado = @Estado, FechaActualizacion = @FechaActualizacion WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, Estado = estado, FechaActualizacion = DateTime.UtcNow });
        return rows > 0;
    }

    protected override string BuildInsertQuery(PlanificacionMensual entity)
    {
        return @"
            INSERT INTO PlanificacionesMensuales (
                PlanificacionId, DocenteId, CursoId, SeccionId, PeriodoAcademicoId,
                Mes, NumeroMes, TituloUnidad, SituacionAprendizaje,
                CompetenciasFundamentales, CompetenciasEspecificas, ContenidosConceptuales,
                ContenidosProcedimentales, ContenidosActitudinales, IndicadoresLogro,
                EstrategiasEnsenanza, RecursosDidacticos, ActividadesEvaluacion, EjesTransversales,
                Observaciones, Estado, FechaCreacion
            ) VALUES (
                @PlanificacionId, @DocenteId, @CursoId, @SeccionId, @PeriodoAcademicoId,
                @Mes, @NumeroMes, @TituloUnidad, @SituacionAprendizaje,
                @CompetenciasFundamentales, @CompetenciasEspecificas, @ContenidosConceptuales,
                @ContenidosProcedimentales, @ContenidosActitudinales, @IndicadoresLogro,
                @EstrategiasEnsenanza, @RecursosDidacticos, @ActividadesEvaluacion, @EjesTransversales,
                @Observaciones, @Estado, @FechaCreacion
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(PlanificacionMensual entity)
    {
        return @"
            UPDATE PlanificacionesMensuales SET
                PlanificacionId = @PlanificacionId, DocenteId = @DocenteId, CursoId = @CursoId,
                SeccionId = @SeccionId, PeriodoAcademicoId = @PeriodoAcademicoId,
                Mes = @Mes, NumeroMes = @NumeroMes, TituloUnidad = @TituloUnidad,
                SituacionAprendizaje = @SituacionAprendizaje,
                CompetenciasFundamentales = @CompetenciasFundamentales,
                CompetenciasEspecificas = @CompetenciasEspecificas,
                ContenidosConceptuales = @ContenidosConceptuales,
                ContenidosProcedimentales = @ContenidosProcedimentales,
                ContenidosActitudinales = @ContenidosActitudinales,
                IndicadoresLogro = @IndicadoresLogro,
                EstrategiasEnsenanza = @EstrategiasEnsenanza,
                RecursosDidacticos = @RecursosDidacticos,
                ActividadesEvaluacion = @ActividadesEvaluacion,
                EjesTransversales = @EjesTransversales,
                Observaciones = @Observaciones,
                Estado = @Estado, FechaActualizacion = @FechaActualizacion
            WHERE Id = @Id";
    }
}
