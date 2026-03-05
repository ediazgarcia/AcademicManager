namespace AcademicManager.Domain.Entities;

/// <summary>
/// Auditoría de Planificación - Registro de cambios, aprobaciones y transiciones de estado
/// Cumple con requerimientos de trazabilidad MINERD
/// </summary>
public class PlanificacionAuditoria
{
    public int Id { get; set; }
    public int PlanificacionId { get; set; }
    public int? UsuarioId { get; set; } // Docente o Supervisor que realizó la acción

    public string Accion { get; set; } = string.Empty; // Crear, Modificar, Enviar, Aprobar, Rechazar, Evaluar
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;

    public string CamposModificados { get; set; } = string.Empty; // JSON con cambios
    public string Observaciones { get; set; } = string.Empty; // Motivo de rechazo, comentarios, etc.

    public DateTime FechaAccion { get; set; } = DateTime.UtcNow;
    public string DireccionIP { get; set; } = string.Empty; // Para seguridad

    // Navigation
    public Planificacion? Planificacion { get; set; }
}
