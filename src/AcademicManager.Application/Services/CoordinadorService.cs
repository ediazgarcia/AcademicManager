using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio facade para operaciones de coordinador.
/// Centraliza la lógica de consultas y reportería específica del coordinador.
/// </summary>
public class CoordinadorService
{
    private readonly ReportingService _reportingService;
    private readonly AuditTrailService _auditTrailService;
    private readonly IDocenteRepository _docenteRepository;

    public CoordinadorService(
        ReportingService reportingService,
        AuditTrailService auditTrailService,
        IDocenteRepository docenteRepository)
    {
        _reportingService = reportingService;
        _auditTrailService = auditTrailService;
        _docenteRepository = docenteRepository;
    }

    /// <summary>
    /// Obtiene dashboard consolidado para coordinador con todos los datos necesarios.
    /// </summary>
    public async Task<ReporteAprobacionesDto> GetDashboardDataAsync(int periodoId)
    {
        try
        {
            var reporteData = await _reportingService.GetPlanificacionStatsAsync(periodoId);

            // Enriquecer datos de docentes en alertas
            if (reporteData.Alertas?.Any() == true)
            {
                var docentes = await _docenteRepository.GetAllAsync();
                foreach (var alerta in reporteData.Alertas)
                {
                    var docente = docentes.FirstOrDefault(d => d.Id == alerta.DocenteId);
                    if (docente != null)
                    {
                        alerta.NombreDocente = $"{docente.Nombres} {docente.Apellidos}";
                    }
                }
            }

            return reporteData;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error obteniendo datos del dashboard", ex);
        }
    }

    /// <summary>
    /// Obtiene reporte detallado de regression de docentes para un período.
    /// </summary>
    public async Task<List<DocenteRegressionReportDto>> GetDocenteRegressionAsync(int periodoId)
    {
        try
        {
            var reporte = await _reportingService.GenerateDocenteRegressionReportAsync(periodoId, new List<Planificacion>());
            return reporte;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error obteniendo regression de docentes", ex);
        }
    }

    /// <summary>
    /// Obtiene datos para exportación de reporte con filtros aplicados.
    /// </summary>
    public async Task<ReporteAprobacionesDto> GetExportDataAsync(
        int periodoId,
        int? gradoId = null,
        string? estado = null)
    {
        try
        {
            return await _reportingService.ExportPlanificacionesStateReportAsync(periodoId, gradoId, estado);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error obteniendo datos para exportación", ex);
        }
    }

    /// <summary>
    /// Obtiene cobertura de planificaciones por curso.
    /// </summary>
    public async Task<List<CoberturaCursoDto>> GetCoberturaByCursoAsync(int periodoId)
    {
        try
        {
            return await _reportingService.GetCoberturaByCursoAsync(periodoId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error obteniendo cobertura por curso", ex);
        }
    }

    /// <summary>
    /// Obtiene estadísticas consolidadas de un período.
    /// </summary>
    public async Task<ReporteAprobacionesDto> GetPeriodStatsAsync(int periodoId)
    {
        return await GetDashboardDataAsync(periodoId);
    }
}
