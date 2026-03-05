using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperPlanificacionDiariaRepository : GenericDapperRepository<PlanificacionDiaria>, IPlanificacionDiariaRepository
{
    protected override string TableName => "PlanificacionesDiarias";

    public DapperPlanificacionDiariaRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<IEnumerable<PlanificacionDiaria>> GetByPlanificacionMensualIdAsync(int planificacionMensualId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM PlanificacionesDiarias WHERE PlanificacionMensualId = @PlanificacionMensualId ORDER BY Fecha ASC";
        return await connection.QueryAsync<PlanificacionDiaria>(sql, new { PlanificacionMensualId = planificacionMensualId });
    }

    public async Task<bool> CambiarEstadoAsync(int id, string estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE PlanificacionesDiarias SET Estado = @Estado, FechaActualizacion = @FechaActualizacion WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, Estado = estado, FechaActualizacion = DateTime.UtcNow });
        return rows > 0;
    }

    protected override string BuildInsertQuery(PlanificacionDiaria entity)
    {
        return @"
            INSERT INTO PlanificacionesDiarias (
                PlanificacionMensualId, Fecha, IntencionPedagogica, ActividadesInicio, TiempoInicioMinutos,
                ActividadesDesarrollo, TiempoDesarrolloMinutos, ActividadesCierre, TiempoCierreMinutos,
                EstrategiasEnsenanza, OrganizacionEstudiantes, VocabularioDia, Recursos, LecturasRecomendadas,
                Observaciones, Estado, FechaCreacion
            ) VALUES (
                @PlanificacionMensualId, @Fecha, @IntencionPedagogica, @ActividadesInicio, @TiempoInicioMinutos,
                @ActividadesDesarrollo, @TiempoDesarrolloMinutos, @ActividadesCierre, @TiempoCierreMinutos,
                @EstrategiasEnsenanza, @OrganizacionEstudiantes, @VocabularioDia, @Recursos, @LecturasRecomendadas,
                @Observaciones, @Estado, @FechaCreacion
            );
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(PlanificacionDiaria entity)
    {
        return @"
            UPDATE PlanificacionesDiarias SET
                PlanificacionMensualId = @PlanificacionMensualId, Fecha = @Fecha, IntencionPedagogica = @IntencionPedagogica,
                ActividadesInicio = @ActividadesInicio, TiempoInicioMinutos = @TiempoInicioMinutos,
                ActividadesDesarrollo = @ActividadesDesarrollo, TiempoDesarrolloMinutos = @TiempoDesarrolloMinutos,
                ActividadesCierre = @ActividadesCierre, TiempoCierreMinutos = @TiempoCierreMinutos,
                EstrategiasEnsenanza = @EstrategiasEnsenanza, OrganizacionEstudiantes = @OrganizacionEstudiantes,
                VocabularioDia = @VocabularioDia, Recursos = @Recursos, LecturasRecomendadas = @LecturasRecomendadas,
                Observaciones = @Observaciones, Estado = @Estado, FechaActualizacion = @FechaActualizacion
            WHERE Id = @Id";
    }
}
