using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application.Services;
using AcademicManager.Application.DTOs;
using AcademicManager.Domain.Entities;
using AutoMapper;

namespace AcademicManager.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "DocenteOrAdmin")]
public class DocenteTareasController : ControllerBase
{
    private readonly TareaService _tareaService;
    private readonly DocenteService _docenteService;
    private readonly CursoService _cursoService;
    private readonly PeriodoAcademicoService _periodoService;
    private readonly IMapper _mapper;
    private readonly ILogger<DocenteTareasController> _logger;

    public DocenteTareasController(
        TareaService tareaService,
        DocenteService docenteService,
        CursoService cursoService,
        PeriodoAcademicoService periodoService,
        IMapper mapper,
        ILogger<DocenteTareasController> logger)
    {
        _tareaService = tareaService;
        _docenteService = docenteService;
        _cursoService = cursoService;
        _periodoService = periodoService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las tareas creadas por un docente
    /// </summary>
    [HttpGet("mis-tareas/{docenteId}")]
    public async Task<ActionResult<IEnumerable<TareaDto>>> ObtenerMisTareas(int docenteId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo tareas del docente {docenteId}");
            
            var docente = await _docenteService.ObtenerPorIdAsync(docenteId);
            if (docente == null)
            {
                _logger.LogWarning($"Docente {docenteId} no encontrado");
                return NotFound(new { message = "Docente no encontrado" });
            }

            var tareas = await _tareaService.ObtenerPorDocenteAsync(docenteId);
            var tareasDto = _mapper.Map<IEnumerable<TareaDto>>(tareas);

            return Ok(tareasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tareas del docente");
            return StatusCode(500, new { message = "Error al obtener las tareas" });
        }
    }

    /// <summary>
    /// Obtiene tareas por curso
    /// </summary>
    [HttpGet("curso/{cursoId}")]
    public async Task<ActionResult<IEnumerable<TareaDto>>> ObtenerTareasPorCurso(int cursoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo tareas del curso {cursoId}");
            
            var tareas = await _tareaService.ObtenerPorCursoAsync(cursoId);
            var tareasDto = _mapper.Map<IEnumerable<TareaDto>>(tareas);

            return Ok(tareasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener tareas por curso");
            return StatusCode(500, new { message = "Error al obtener las tareas del curso" });
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
    /// Crea una nueva tarea (solo Docente propietario)
    /// </summary>
    [HttpPost("crear")]
    public async Task<ActionResult<TareaDto>> CrearTarea([FromBody] CreateTareaDto createTareaDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar que el docente existe
            var docente = await _docenteService.ObtenerPorIdAsync(createTareaDto.DocenteId);
            if (docente == null)
            {
                return NotFound(new { message = "Docente no encontrado" });
            }

            // Validar que el curso existe
            var curso = await _cursoService.ObtenerPorIdAsync(createTareaDto.CursoId);
            if (curso == null)
            {
                return NotFound(new { message = "Curso no encontrado" });
            }

            // Validar que el periodo existe
            var periodo = await _periodoService.ObtenerPorIdAsync(createTareaDto.PeriodoAcademicoId);
            if (periodo == null)
            {
                return NotFound(new { message = "Periodo académico no encontrado" });
            }

            // Validar fecha de entrega
            if (createTareaDto.FechaEntrega <= DateTime.UtcNow)
            {
                return BadRequest(new { message = "La fecha de entrega debe ser en el futuro" });
            }

            var tarea = _mapper.Map<Tarea>(createTareaDto);
            var tareaId = await _tareaService.CrearAsync(tarea);

            var tareaCreada = await _tareaService.ObtenerPorIdAsync(tareaId);
            var tareaDto = _mapper.Map<TareaDto>(tareaCreada);

            return CreatedAtAction(nameof(ObtenerTarea), new { tareaId = tareaId }, tareaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear la tarea");
            return StatusCode(500, new { message = "Error al crear la tarea" });
        }
    }

    /// <summary>
    /// Actualiza una tarea existente
    /// </summary>
    [HttpPut("actualizar")]
    public async Task<ActionResult<TareaDto>> ActualizarTarea([FromBody] UpdateTareaDto updateTareaDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tarea = await _tareaService.ObtenerPorIdAsync(updateTareaDto.Id);
            if (tarea == null)
            {
                return NotFound(new { message = "Tarea no encontrada" });
            }

            // Validar que solo el docente propietario pueda actualizar
            var docenteStr = HttpContext.Session.GetString("DocenteId");
            if (!int.TryParse(docenteStr, out var docenteId) || docenteId != tarea.DocenteId)
            {
                return Forbid("No tienes permiso para actualizar esta tarea");
            }

            _mapper.Map(updateTareaDto, tarea);
            var actualizado = await _tareaService.ActualizarAsync(tarea);

            if (!actualizado)
            {
                return BadRequest(new { message = "No se pudo actualizar la tarea" });
            }

            var tareaDto = _mapper.Map<TareaDto>(tarea);
            return Ok(tareaDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar la tarea");
            return StatusCode(500, new { message = "Error al actualizar la tarea" });
        }
    }

    /// <summary>
    /// Elimina una tarea
    /// </summary>
    [HttpDelete("{tareaId}")]
    public async Task<IActionResult> EliminarTarea(int tareaId)
    {
        try
        {
            var tarea = await _tareaService.ObtenerPorIdAsync(tareaId);
            if (tarea == null)
            {
                return NotFound(new { message = "Tarea no encontrada" });
            }

            // Validar que solo el docente propietario pueda eliminar
            var docenteStr = HttpContext.Session.GetString("DocenteId");
            if (!int.TryParse(docenteStr, out var docenteId) || docenteId != tarea.DocenteId)
            {
                return Forbid("No tienes permiso para eliminar esta tarea");
            }

            var eliminada = await _tareaService.EliminarAsync(tareaId);
            if (!eliminada)
            {
                return BadRequest(new { message = "No se pudo eliminar la tarea" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar la tarea");
            return StatusCode(500, new { message = "Error al eliminar la tarea" });
        }
    }

    /// <summary>
    /// Obtiene todas las entregas de una tarea
    /// </summary>
    [HttpGet("{tareaId}/entregas")]
    public async Task<ActionResult<IEnumerable<EntregaTareaDto>>> ObtenerEntregasPorTarea(int tareaId)
    {
        try
        {
            var tarea = await _tareaService.ObtenerPorIdAsync(tareaId);
            if (tarea == null)
            {
                return NotFound(new { message = "Tarea no encontrada" });
            }

            var entregas = await _tareaService.ObtenerEntregasPorTareaAsync(tareaId);
            var entregasDto = _mapper.Map<IEnumerable<EntregaTareaDto>>(entregas);

            return Ok(entregasDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entregas de la tarea");
            return StatusCode(500, new { message = "Error al obtener las entregas" });
        }
    }

    /// <summary>
    /// Califica una entrega de tarea
    /// </summary>
    [HttpPost("calificar")]
    public async Task<IActionResult> CalificarEntrega([FromBody] CalificarEntregaTareaDto calificacionDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var resultado = await _tareaService.CalificarEntregaAsync(
                calificacionDto.EntregaId,
                calificacionDto.Puntos,
                calificacionDto.Retroalimentacion);

            if (!resultado)
            {
                return BadRequest(new { message = "No se pudo calificar la entrega" });
            }

            return Ok(new { message = "Entrega calificada correctamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calificar la entrega");
            return StatusCode(500, new { message = "Error al calificar la entrega" });
        }
    }
}
