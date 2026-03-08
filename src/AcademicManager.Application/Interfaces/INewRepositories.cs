using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

/// <summary>
/// Repositorio para gestionar auditoría de cambios en calificaciones.
/// </summary>
public interface IGradeAuditTrailRepository
{
    Task<GradeAuditTrail?> GetByIdAsync(int id);
    Task<IEnumerable<GradeAuditTrail>> GetAllAsync();
    Task<IEnumerable<GradeAuditTrail>> GetByEntregaIdAsync(int entregaTareaId);
    Task<IEnumerable<GradeAuditTrail>> GetByDocenteIdAsync(int docenteId);
    Task<IEnumerable<GradeAuditTrail>> GetByDateRangeAsync(DateTime desde, DateTime hasta);
    Task<int> CreateAsync(GradeAuditTrail auditTrail);
    Task<bool> UpdateAsync(GradeAuditTrail auditTrail);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Repositorio para gestionar plantillas de retroalimentación.
/// </summary>
public interface IFeedbackTemplateRepository
{
    Task<FeedbackTemplate?> GetByIdAsync(int id);
    Task<IEnumerable<FeedbackTemplate>> GetAllAsync();
    Task<IEnumerable<FeedbackTemplate>> GetByDocenteIdAsync(int docenteId);
    Task<IEnumerable<FeedbackTemplate>> GetByMateriaAsync(string materia);
    Task<int> CreateAsync(FeedbackTemplate template);
    Task<bool> UpdateAsync(FeedbackTemplate template);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Repositorio para gestionar notificaciones de estudiantes.
/// </summary>
public interface IStudentNotificationRepository
{
    Task<StudentNotification?> GetByIdAsync(int id);
    Task<IEnumerable<StudentNotification>> GetAllAsync();
    Task<IEnumerable<StudentNotification>> GetByAlumnoIdAsync(int alumnoId);
    Task<IEnumerable<StudentNotification>> GetUnreadByAlumnoIdAsync(int alumnoId);
    Task<int> CreateAsync(StudentNotification notification);
    Task<bool> UpdateAsync(StudentNotification notification);
    Task<bool> DeleteAsync(int id);
}

/// <summary>
/// Repositorio para gestionar templates de reportes.
/// </summary>
public interface IReportTemplateRepository
{
    Task<ReportTemplate?> GetByIdAsync(int id);
    Task<IEnumerable<ReportTemplate>> GetAllAsync();
    Task<IEnumerable<ReportTemplate>> GetByCoordinadorIdAsync(int coordinadorId);
    Task<IEnumerable<ReportTemplate>> GetByTipoAsync(string tipoReporte);
    Task<int> CreateAsync(ReportTemplate template);
    Task<bool> UpdateAsync(ReportTemplate template);
    Task<bool> DeleteAsync(int id);
}
