using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application.Services;
using AcademicManager.Application.DTOs;
using AcademicManager.Domain.Entities;
using AutoMapper;

namespace AcademicManager.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "AlumnoOnly")]
public class AlumnoTareasController : ControllerBase
{
    private readonly TareaService _tareaService;
    private readonly AlumnoService _alumnoService;
    private readonly PeriodoAcademicoService _periodoService;
    private readonly IMapper _mapper;
    private readonly ILogger<AlumnoTareasController> _logger;

    public AlumnoTareasController(
        TareaService tareaService,
        AlumnoService alumnoService,
        PeriodoAcademicoService periodoService,
        IMapper mapper,
        ILogger<AlumnoTareasController> logger)
    {
        _tareaService = tareaService;
        _alumnoService = alumnoService;
        _periodoService = periodoService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las tareas asignadas a un alumno en un período
    /// </summary>
    [HttpGet("mis-tareas/{alumnoId}/{periodoId}")]
    public async Task<ActionResult<IEnumerable<TareaDto>>> ObtenerMisTareas(int alumnoId, int periodoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo tareas del alumno {alumnoId} para el período {periodoId}");
            
            // Validar que el alumno existe
            var alumno = await _alumnoService.ObtenerPorIdAsync(alumnoId);
            if (alumno == null)
            {
                _logger.LogWarning($"Alumno {alumnoId} no encontrado");
                return NotFound(new { message = "Alumno no encontrado" });
            }

            // Validar que el alumno tiene una sección asignada
            if (!alumno.SeccionId.HasValue)
            {
                return BadRequest(new { message = "No estás asignado a ninguna sección" });
            }

            // Obtener tareas del periodo
            var tareas = await _tareaService.ObtenerPorAlumnoAsync(alumnoId, periodoId);
            
            if (!tareas.Any())
            {
                _logger.LogInformation($"No hay tareas para el alumno {alumnoId} en el período {periodoId}");
                return Ok(new List<TareaDto>());
            }

            var tareasDto = _mapper.Map<IEnumerable<TareaDto>>(tareas);

            // Agregar información de entregas
            foreach (var tareaDto in tareasDto)
            {
                var entrega = await _tareaService.ObtenerEntregaPorTareaYAlumnoAsync(tareaDto.Id, alumnoId);
                if (entrega != null)
                {
                    tareaDto.EntregaInfo = _mapper.Map<EntregaTareaDto>(entrega);
                }
            }

            return Ok(tareasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tareas del alumno");
            return StatusCode(500, new { message = "Error al obtener las tareas" });
        }
    }

    /// <summary>
    /// Obtiene los detalles de una tarea específica
    /// </summary>
    [HttpGet("{tareaId}")]
    public async Task<ActionResult<TareaDto>> ObtenerTarea(int tareaId)
    {
        try
        {
            var tarea = await _tareaService.ObtenerPorIdAsync(tareaId);
            if (tarea == null)
            {
                return NotFound(new { message = "Tarea no encontrada" });
            }

            var tareaDto = _mapper.Map<TareaDto>(tarea);
            return Ok(tareaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalles de la tarea");
            return StatusCode(500, new { message = "Error al obtener la tarea" });
        }
    }

    /// <summary>
    /// Crea una entrega de tarea (para que el alumno entregue su trabajo)
    /// </summary>
    [HttpPost("entregar")]
    public async Task<ActionResult<EntregaTareaDto>> EntregarTarea([FromBody] CreateEntregaTareaDto entregarDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Obtener la tarea
            var tarea = await _tareaService.ObtenerPorIdAsync(entregarDto.TareaId);
            if (tarea == null)
            {
                return NotFound(new { message = "Tarea no encontrada" });
            }

            // Validar que no esté vencida (a menos que permita entregas tardías)
            if (DateTime.UtcNow > tarea.FechaEntrega && !tarea.PermiteEntregaTardia)
            {
                return BadRequest(new { message = "La fecha de entrega de esta tarea ha vencido y no se aceptan entregas tardías" });
            }

            // Crear la entrega
            var entrega = _mapper.Map<EntregaTarea>(entregarDto);
            int entregaId = await _tareaService.CrearEntregaAsync(entrega, tarea);

            var entregaCreada = await _tareaService.ObtenerEntregaAsync(entregaId);
            var entregaDto = _mapper.Map<EntregaTareaDto>(entregaCreada);

            return CreatedAtAction(nameof(ObtenerEntrega), new { entregaId = entregaId }, entregaDto);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Operación inválida al entregar tarea");
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al entregar tarea");
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al entregar la tarea");
            return StatusCode(500, new { message = "Error al entregar la tarea" });
        }
    }

    /// <summary>
    /// Obtiene el estado de una entrega de tarea
    /// </summary>
    [HttpGet("entrega/{entregaId}")]
    public async Task<ActionResult<EntregaTareaDto>> ObtenerEntrega(int entregaId)
    {
        try
        {
            var entrega = await _tareaService.ObtenerEntregaAsync(entregaId);
            if (entrega == null)
            {
                return NotFound(new { message = "Entrega no encontrada" });
            }

            var entregaDto = _mapper.Map<EntregaTareaDto>(entrega);
            return Ok(entregaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener la entrega");
            return StatusCode(500, new { message = "Error al obtener la entrega" });
        }
    }

    /// <summary>
    /// Obtiene todas las entregas de un alumno
    /// </summary>
    [HttpGet("mis-entregas/{alumnoId}")]
    public async Task<ActionResult<IEnumerable<EntregaTareaDto>>> ObtenerMisEntregas(int alumnoId)
    {
        try
        {
            var alumno = await _alumnoService.ObtenerPorIdAsync(alumnoId);
            if (alumno == null)
            {
                return NotFound(new { message = "Alumno no encontrado" });
            }

            var entregas = await _tareaService.ObtenerEntregasPorAlumnoAsync(alumnoId);
            var entregasDto = _mapper.Map<IEnumerable<EntregaTareaDto>>(entregas);

            return Ok(entregasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entregas del alumno");
            return StatusCode(500, new { message = "Error al obtener las entregas" });
        }
    }

    /// <summary>
    /// Obtiene la calificación total del alumno en un período
    /// </summary>
    [HttpGet("calificacion/{alumnoId}/{periodoId}")]
    public async Task<ActionResult<object>> ObtenerCalificacionPeriodo(int alumnoId, int periodoId)
    {
        try
        {
            var alumno = await _alumnoService.ObtenerPorIdAsync(alumnoId);
            if (alumno == null)
            {
                return NotFound(new { message = "Alumno no encontrado" });
            }

            var (puntos, puntosMaximos, porcentaje) = await _tareaService.GetCalificacionPeriodoAsync(alumnoId, periodoId);

            return Ok(new
            {
                puntosObtenidos = puntos,
                puntosMaximos = puntosMaximos,
                porcentaje = porcentaje,
                calificacion = GetCalificacionLiteral(porcentaje)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener calificación del período");
            return StatusCode(500, new { message = "Error al obtener calificación" });
        }
    }

    private string GetCalificacionLiteral(double porcentaje)
    {
        return porcentaje switch
        {
            >= 90 => "Excelente",
            >= 80 => "Muy Bien",
            >= 70 => "Bien",
            >= 60 => "Satisfactorio",
            >= 50 => "Regular",
            _ => "Insuficiente"
        };
    }
}
