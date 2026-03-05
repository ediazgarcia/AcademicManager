namespace AcademicManager.Domain.Entities;

public class PlanificacionDiaria
{
    public int Id { get; set; }
    public int PlanificacionMensualId { get; set; }
    public DateTime Fecha { get; set; }
    
    public string IntencionPedagogica { get; set; } = string.Empty;
    
    // Momentos de la clase
    public string ActividadesInicio { get; set; } = string.Empty;
    public int TiempoInicioMinutos { get; set; } = 15;
    
    public string ActividadesDesarrollo { get; set; } = string.Empty;
    public int TiempoDesarrolloMinutos { get; set; } = 20;
    
    public string ActividadesCierre { get; set; } = string.Empty;
    public int TiempoCierreMinutos { get; set; } = 10;
    
    public string EstrategiasEnsenanza { get; set; } = string.Empty;
    public string OrganizacionEstudiantes { get; set; } = string.Empty;
    public string VocabularioDia { get; set; } = string.Empty;
    public string Recursos { get; set; } = string.Empty;
    public string LecturasRecomendadas { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    
    public string Estado { get; set; } = "Borrador"; 
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaActualizacion { get; set; }

    // Navigation
    public PlanificacionMensual? PlanificacionMensual { get; set; }
}
