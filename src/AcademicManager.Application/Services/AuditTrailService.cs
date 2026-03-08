using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio para gestionar auditoría de cambios en calificaciones y otros eventos.
/// Registra historial completo para cumplimiento y análisis.
/// </summary>
public class AuditTrailService
{
    private readonly IGradeAuditTrailRepository _auditRepository;
    private readonly IEntregaTareaRepository _entregaTareaRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public AuditTrailService(
        IGradeAuditTrailRepository auditRepository,
        IEntregaTareaRepository entregaTareaRepository,
        IUsuarioRepository usuarioRepository)
    {
        _auditRepository = auditRepository;
        _entregaTareaRepository = entregaTareaRepository;
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Registra un cambio de calificación en la auditoría.
    /// </summary>
    public async Task<(bool Success, string Message)> LogGradeChangeAsync(
        int entregaId,
        decimal? notaAnterior,
        decimal notaNueva,
        int usuarioId,
        string razon = "Ajuste de calificación")
    {
        try
        {
            var audit = new GradeAuditTrail
            {
                EntregaTareaId = entregaId,
                DocenteId = usuarioId,
                NotaAnterior = notaAnterior,
                NotaNueva = notaNueva,
                Razon = razon,
                Timestamp = DateTime.UtcNow
            };

            await _auditRepository.CreateAsync(audit);

            return (true, "Cambio de calificación registrado en auditoría.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar auditoría: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene el historial completo de cambios para una entrega.
    /// </summary>
    public async Task<List<GradeChangeHistoryDto>> GetAuditTrailAsync(int entregaId)
    {
        var audits = (await _auditRepository.GetByEntregaIdAsync(entregaId))
            .OrderByDescending(a => a.Timestamp)
            .ToList();

        var historialy = new List<GradeChangeHistoryDto>();

        foreach (var audit in audits)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(audit.DocenteId);

            historialy.Add(new GradeChangeHistoryDto
            {
                Id = audit.Id,
                EntregaTareaId = audit.EntregaTareaId,
                DocenteId = audit.DocenteId,
                NombreDocente = usuario != null
                    ? usuario.NombreUsuario
                    : "Sistema",
                NotaAnterior = audit.NotaAnterior,
                NotaNueva = audit.NotaNueva,
                Razon = audit.Razon,
                Timestamp = audit.Timestamp
            });
        }

        return historialy;
    }

    /// <summary>
    /// Obtiene resumen de auditoría para un período.
    /// </summary>
    public async Task<(int TotalCambios, int TotalEntregas, decimal CambioPromedio)> GetAuditSummaryAsync(
        DateTime desde,
        DateTime hasta)
    {
        var audits = (await _auditRepository.GetByDateRangeAsync(desde, hasta))
            .ToList();

        var entregas = audits.Select(a => a.EntregaTareaId).Distinct().Count();
        var totalCambios = audits.Count;
        var cambioPromedio = totalCambios > 0
            ? audits.Average(a => Math.Abs(a.NotaNueva - (a.NotaAnterior ?? 0)))
            : 0m;

        return (totalCambios, entregas, cambioPromedio);
    }

    /// <summary>
    /// Obtiene cambios realizados por un docente específico.
    /// </summary>
    public async Task<List<GradeChangeHistoryDto>> GetChangesByDocenteAsync(int docenteId, int? ultimosNdias = 30)
    {
        var desde = ultimosNdias.HasValue
            ? DateTime.UtcNow.AddDays(-ultimosNdias.Value)
            : DateTime.MinValue;

        var audits = (await _auditRepository.GetByDocenteIdAsync(docenteId))
            .Where(a => a.Timestamp >= desde)
            .OrderByDescending(a => a.Timestamp)
            .ToList();

        var historia = new List<GradeChangeHistoryDto>();

        foreach (var audit in audits)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(audit.DocenteId);

            historia.Add(new GradeChangeHistoryDto
            {
                Id = audit.Id,
                EntregaTareaId = audit.EntregaTareaId,
                DocenteId = audit.DocenteId,
                NombreDocente = usuario?.NombreUsuario ?? "Sistema",
                NotaAnterior = audit.NotaAnterior,
                NotaNueva = audit.NotaNueva,
                Razon = audit.Razon,
                Timestamp = audit.Timestamp
            });
        }

        return historia;
    }

    /// <summary>
    /// Verifica si hay cambios sospechosos (grandes diferencias).
    /// </summary>
    public async Task<List<GradeChangeHistoryDto>> GetSuspiciousChangesAsync(decimal umbralDiferencia = 30m)
    {
        var audits = (await _auditRepository.GetAllAsync())
            .Where(a => Math.Abs(a.NotaNueva - (a.NotaAnterior ?? 0)) >= umbralDiferencia)
            .OrderByDescending(a => a.Timestamp)
            .Take(100)
            .ToList();

        var historia = new List<GradeChangeHistoryDto>();

        foreach (var audit in audits)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(audit.DocenteId);

            historia.Add(new GradeChangeHistoryDto
            {
                Id = audit.Id,
                EntregaTareaId = audit.EntregaTareaId,
                DocenteId = audit.DocenteId,
                NombreDocente = usuario?.NombreUsuario ?? "Sistema",
                NotaAnterior = audit.NotaAnterior,
                NotaNueva = audit.NotaNueva,
                Razon = audit.Razon,
                Timestamp = audit.Timestamp
            });
        }

        return historia;
    }

    /// <summary>
    /// Exporta auditoría completa para compliance.
    /// </summary>
    public async Task<List<GradeChangeHistoryDto>> ExportAuditTrailAsync(
        string tipo,
        int? periodoId = null,
        DateTime? desde = null,
        DateTime? hasta = null)
    {
        var audits = (await _auditRepository.GetAllAsync())
            .ToList();

        // Filtrar por rango de fechas si se proporciona
        if (desde.HasValue && hasta.HasValue)
        {
            audits = audits
                .Where(a => a.Timestamp >= desde && a.Timestamp <= hasta)
                .ToList();
        }

        var historia = new List<GradeChangeHistoryDto>();

        foreach (var audit in audits)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(audit.DocenteId);

            historia.Add(new GradeChangeHistoryDto
            {
                Id = audit.Id,
                EntregaTareaId = audit.EntregaTareaId,
                DocenteId = audit.DocenteId,
                NombreDocente = usuario?.NombreUsuario ?? "Sistema",
                NotaAnterior = audit.NotaAnterior,
                NotaNueva = audit.NotaNueva,
                Razon = audit.Razon,
                Timestamp = audit.Timestamp
            });
        }

        return historia.OrderByDescending(h => h.Timestamp).ToList();
    }
}
