using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application.Services;
using AcademicManager.Application.DTOs;
using AutoMapper;

namespace AcademicManager.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Policy = "DocenteOrAdmin")]
public class DocentesController : ControllerBase
{
    private readonly DocenteService _docenteService;
    private readonly CursoService _cursoService;
    private readonly HorarioService _horarioService;
    private readonly PlanificacionService _planificacionService;
    private readonly IMapper _mapper;
    private readonly ILogger<DocentesController> _logger;

    public DocentesController(
        DocenteService docenteService,
        CursoService cursoService,
        HorarioService horarioService,
        PlanificacionService planificacionService,
        IMapper mapper,
        ILogger<DocentesController> logger)
    {
        _docenteService = docenteService;
        _cursoService = cursoService;
        _horarioService = horarioService;
        _planificacionService = planificacionService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el perfil del docente
    /// </summary>
    [HttpGet("{docenteId}")]
    public async Task<ActionResult<DocenteDto>> ObtenerPerfil(int docenteId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo perfil del docente {docenteId}");
            
            var docente = await _docenteService.ObtenerPorIdAsync(docenteId);
            if (docente == null)
            {
                return NotFound(new { message = "Docente no encontrado" });
            }

            var docenteDto = _mapper.Map<DocenteDto>(docente);
            return Ok(docenteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil del docente");
            return StatusCode(500, new { message = "Error al obtener el perfil" });
        }
    }

    /// <summary>
    /// Obtiene todos los docentes
    /// </summary>
    [HttpGet("")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<DocenteDto>>> ObtenerTodos()
    {
        try
        {
            var docentes = await _docenteService.ObtenerTodosAsync();
            var docentesDto = _mapper.Map<IEnumerable<DocenteDto>>(docentes);
            return Ok(docentesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener docentes");
            return StatusCode(500, new { message = "Error al obtener docentes" });
        }
    }

    /// <summary>
    /// Obtiene los cursos asignados a un docente
    /// </summary>
    [HttpGet("{docenteId}/cursos")]
    public async Task<ActionResult<IEnumerable<CursoDto>>> ObtenerCursosDocente(int docenteId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo cursos del docente {docenteId}");
            
            var docente = await _docenteService.ObtenerPorIdAsync(docenteId);
            if (docente == null)
            {
                return NotFound(new { message = "Docente no encontrado" });
            }

            // Obtener cursos del docente a través de la relación en horarios
            var horarios = await _horarioService.ObtenerPorDocenteAsync(docenteId);
            
            if (!horarios.Any())
            {
                return Ok(new List<CursoDto>());
            }

            var cursoIds = horarios.Select(h => h.CursoId).Distinct();
            var cursos = new List<CursoDto>();

            foreach (var cursoId in cursoIds)
            {
                var curso = await _cursoService.ObtenerPorIdAsync(cursoId);
                if (curso != null)
                {
                    cursos.Add(_mapper.Map<CursoDto>(curso));
                }
            }

            return Ok(cursos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cursos del docente");
            return StatusCode(500, new { message = "Error al obtener los cursos" });
        }
    }

    /// <summary>
    /// Obtiene los horarios asignados a un docente
    /// </summary>
    [HttpGet("{docenteId}/horarios")]
    public async Task<ActionResult<IEnumerable<HorarioDto>>> ObtenerHorariosDocente(int docenteId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo horarios del docente {docenteId}");
            
            var docente = await _docenteService.ObtenerPorIdAsync(docenteId);
            if (docente == null)
            {
                return NotFound(new { message = "Docente no encontrado" });
            }

            var horarios = await _horarioService.ObtenerPorDocenteAsync(docenteId);
            var horariosDto = _mapper.Map<IEnumerable<HorarioDto>>(horarios);

            return Ok(horariosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener horarios del docente");
            return StatusCode(500, new { message = "Error al obtener los horarios" });
        }
    }

    /// <summary>
    /// Obtiene las planificaciones de un docente
    /// </summary>
    [HttpGet("{docenteId}/planificaciones")]
    public async Task<ActionResult<IEnumerable<PlanificacionDto>>> ObtenerPlanificacionesDocente(int docenteId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo planificaciones del docente {docenteId}");
            
            var docente = await _docenteService.ObtenerPorIdAsync(docenteId);
            if (docente == null)
            {
                return NotFound(new { message = "Docente no encontrado" });
            }

            var planificaciones = await _planificacionService.ObtenerPorDocenteAsync(docenteId);
            var planificacionesDto = _mapper.Map<IEnumerable<PlanificacionDto>>(planificaciones);

            return Ok(planificacionesDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener planificaciones del docente");
            return StatusCode(500, new { message = "Error al obtener las planificaciones" });
        }
    }

    /// <summary>
    /// Actualiza el perfil del docente
    /// </summary>
    [HttpPut("{docenteId}")]
    public async Task<ActionResult<DocenteDto>> ActualizarPerfil(int docenteId, [FromBody] UpdateDocenteDto updateDto)
    {
        try
        {
            var docente = await _docenteService.ObtenerPorIdAsync(docenteId);
            if (docente == null)
            {
                return NotFound(new { message = "Docente no encontrado" });
            }

            _mapper.Map(updateDto, docente);
            var actualizado = await _docenteService.ActualizarAsync(docente);

            if (!actualizado)
            {
                return BadRequest(new { message = "No se pudo actualizar el perfil" });
            }

            var docenteDto = _mapper.Map<DocenteDto>(docente);
            return Ok(docenteDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el perfil del docente");
            return StatusCode(500, new { message = "Error al actualizar el perfil" });
        }
    }
}
