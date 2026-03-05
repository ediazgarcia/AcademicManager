namespace AcademicManager.Application.DTOs;

public class DocenteDto
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaContratacion { get; set; }
    public string NombreCompleto => $"{Nombres} {Apellidos}";
}

public class UpdateDocenteDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class CursoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public int? GradoId { get; set; }
    public string? GradoNombre { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int HorasSemanales { get; set; }
    public bool Activo { get; set; }
}

public class GradoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public int Orden { get; set; }
    public bool Activo { get; set; }
}

public class SeccionDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int? GradoId { get; set; }
    public int CupoMaximo { get; set; }
    public int CupoActual { get; set; }
    public bool Activo { get; set; }
}

public class PeriodoAcademicoDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public bool Activo { get; set; }
    public bool EsActual { get; set; }
}

public class HorarioDto
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public int DocenteId { get; set; }
    public int SeccionId { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public TimeSpan HoraInicio { get; set; }
    public TimeSpan HoraFin { get; set; }
    public string Aula { get; set; } = string.Empty;
}

public class PlanificacionDto
{
    public int Id { get; set; }
    public int DocenteId { get; set; }
    public int CursoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class EvaluacionDto
{
    public int Id { get; set; }
    public int CursoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Peso { get; set; }
    public string Tipo { get; set; } = string.Empty;
}

public class CalificacionDto
{
    public int Id { get; set; }
    public int AlumnoId { get; set; }
    public int EvaluacionId { get; set; }
    public decimal Valor { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}

public class AsistenciaDto
{
    public int Id { get; set; }
    public int AlumnoId { get; set; }
    public DateTime Fecha { get; set; }
    public bool Presente { get; set; }
    public string Observaciones { get; set; } = string.Empty;
}

public class UsuarioDto
{
    public int Id { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
}

public class TareaDto
{
    public int Id { get; set; }
    public int PlanificacionId { get; set; }
    public int CursoId { get; set; }
    public string? CursoNombre { get; set; }
    public int PeriodoAcademicoId { get; set; }
    public string? PeriodoNombre { get; set; }
    public int DocenteId { get; set; }
    public string? DocenteNombre { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaPublicacion { get; set; }
    public DateTime FechaEntrega { get; set; }
    public int PuntosMaximos { get; set; } = 100;
    public bool PermiteEntregaTardia { get; set; } = false;
    public int DiasTardiosPermitidos { get; set; } = 0;
    public string TipoArchivoPermitido { get; set; } = string.Empty;
    public long TamanoMaximoArchivo { get; set; } = 10485760;
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
    public bool EstaVencida => DateTime.UtcNow > FechaEntrega;
    public int DiasRestantes => Math.Max(0, (FechaEntrega - DateTime.UtcNow).Days);
    public EntregaTareaDto? EntregaInfo { get; set; }
}

public class CreateTareaDto
{
    public int PlanificacionId { get; set; }
    public int CursoId { get; set; }
    public int PeriodoAcademicoId { get; set; }
    public int DocenteId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaEntrega { get; set; }
    public int PuntosMaximos { get; set; } = 100;
    public bool PermiteEntregaTardia { get; set; } = false;
    public int DiasTardiosPermitidos { get; set; } = 0;
    public string TipoArchivoPermitido { get; set; } = "pdf,doc,docx,xls,xlsx,ppt,pptx,jpg,jpeg,png,gif,zip";
    public long TamanoMaximoArchivo { get; set; } = 10485760;
}

public class UpdateTareaDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaEntrega { get; set; }
    public int PuntosMaximos { get; set; } = 100;
    public bool PermiteEntregaTardia { get; set; } = false;
    public int DiasTardiosPermitidos { get; set; } = 0;
    public bool Activa { get; set; } = true;
}

public class EntregaTareaDto
{
    public int Id { get; set; }
    public int TareaId { get; set; }
    public string? TareaTitulo { get; set; }
    public int AlumnoId { get; set; }
    public string? AlumnoNombre { get; set; }
    public string? NombreArchivo { get; set; }
    public string? RutaArchivo { get; set; }
    public string? TipoArchivo { get; set; }
    public long? TamanoArchivo { get; set; }
    public string? Comentarios { get; set; }
    public DateTime FechaEntrega { get; set; }
    public bool EsTardia { get; set; }
    public DateTime? FechaCalificacion { get; set; }
    public int? Puntos { get; set; }
    public string? Retroalimentacion { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaActualizacion { get; set; }
}

public class CreateEntregaTareaDto
{
    public int TareaId { get; set; }
    public int AlumnoId { get; set; }
    public string? NombreArchivo { get; set; }
    public string? RutaArchivo { get; set; }
    public string? TipoArchivo { get; set; }
    public long? TamanoArchivo { get; set; }
    public string? Comentarios { get; set; }
}

public class CalificarEntregaTareaDto
{
    public int EntregaId { get; set; }
    public int Puntos { get; set; }
    public string? Retroalimentacion { get; set; }
}
