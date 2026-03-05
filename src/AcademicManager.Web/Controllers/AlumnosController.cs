using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AcademicManager.Application.Services;
using AcademicManager.Application.DTOs;
using AutoMapper;

namespace AcademicManager.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AlumnosController : ControllerBase
{
    private readonly AlumnoService _alumnoService;
    private readonly GradoService _gradoService;
    private readonly SeccionService _seccionService;
    private readonly HorarioService _horarioService;
    private readonly TareaService _tareaService;
    private readonly PeriodoAcademicoService _periodoService;
    private readonly IMapper _mapper;
    private readonly ILogger<AlumnosController> _logger;

    public AlumnosController(
        AlumnoService alumnoService,
        GradoService gradoService,
        SeccionService seccionService,
        HorarioService horarioService,
        TareaService tareaService,
        PeriodoAcademicoService periodoService,
        IMapper mapper,
        ILogger<AlumnosController> logger)
    {
        _alumnoService = alumnoService;
        _gradoService = gradoService;
        _seccionService = seccionService;
        _horarioService = horarioService;
        _tareaService = tareaService;
        _periodoService = periodoService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el perfil del alumno
    /// </summary>
    [HttpGet("{alumnoId}")]
    public async Task<ActionResult<AlumnoDto>> ObtenerPerfil(int alumnoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo perfil del alumno {alumnoId}");
            
            var alumno = await _alumnoService.ObtenerPorIdAsync(alumnoId);
            if (alumno == null)
            {
                return NotFound(new { message = "Alumno no encontrado" });
            }

            var alumnoDto = _mapper.Map<AlumnoDto>(alumno);
            
            // Agregar información de grado y sección
            if (alumno.GradoId.HasValue)
            {
                var grado = await _gradoService.ObtenerPorIdAsync(alumno.GradoId.Value);
                alumnoDto.GradoNombre = grado?.Nombre;
            }

            if (alumno.SeccionId.HasValue)
            {
                var seccion = await _seccionService.ObtenerPorIdAsync(alumno.SeccionId.Value);
                alumnoDto.SeccionNombre = seccion?.Nombre;
            }

            return Ok(alumnoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil del alumno");
            return StatusCode(500, new { message = "Error al obtener el perfil" });
        }
    }

    /// <summary>
    /// Obtiene todos los alumnos (solo Admin)
    /// </summary>
    [HttpGet("")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<IEnumerable<AlumnoDto>>> ObtenerTodos()
    {
        try
        {
            var alumnos = await _alumnoService.ObtenerTodosAsync();
            var alumnosDto = _mapper.Map<IEnumerable<AlumnoDto>>(alumnos);
            return Ok(alumnosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alumnos");
            return StatusCode(500, new { message = "Error al obtener alumnos" });
        }
    }

    /// <summary>
    /// Obtiene los alumnos por grado
    /// </summary>
    [HttpGet("grado/{gradoId}")]
    [Authorize(Policy = "CanManageAlumnos")]
    public async Task<ActionResult<IEnumerable<AlumnoDto>>> ObtenerAlumnosPorGrado(int gradoId)
    {
        try
        {
            var grado = await _gradoService.ObtenerPorIdAsync(gradoId);
            if (grado == null)
            {
                return NotFound(new { message = "Grado no encontrado" });
            }

            var alumnos = await _alumnoService.ObtenerPorGradoAsync(gradoId);
            var alumnosDto = _mapper.Map<IEnumerable<AlumnoDto>>(alumnos);

            return Ok(alumnosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alumnos por grado");
            return StatusCode(500, new { message = "Error al obtener alumnos" });
        }
    }

    /// <summary>
    /// Obtiene los alumnos por sección
    /// </summary>
    [HttpGet("seccion/{seccionId}")]
    [Authorize(Policy = "DocenteOrAdmin")]
    public async Task<ActionResult<IEnumerable<AlumnoDto>>> ObtenerAlumnosPorSeccion(int seccionId)
    {
        try
        {
            var seccion = await _seccionService.ObtenerPorIdAsync(seccionId);
            if (seccion == null)
            {
                return NotFound(new { message = "Sección no encontrada" });
            }

            var alumnos = await _alumnoService.ObtenerPorSeccionAsync(seccionId);
            var alumnosDto = _mapper.Map<IEnumerable<AlumnoDto>>(alumnos);

            return Ok(alumnosDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alumnos por sección");
            return StatusCode(500, new { message = "Error al obtener alumnos" });
        }
    }

    /// <summary>
    /// Obtiene el dashboard del alumno con información relevante
    /// </summary>
    [HttpGet("{alumnoId}/dashboard")]
    public async Task<ActionResult<object>> ObtenerDashboard(int alumnoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo dashboard del alumno {alumnoId}");
            
            var alumno = await _alumnoService.ObtenerPorIdAsync(alumnoId);
            if (alumno == null)
            {
                return NotFound(new { message = "Alumno no encontrado" });
            }

            // Obtener información básica
            var alumnoDto = _mapper.Map<AlumnoDto>(alumno);
            
            // Obtener período actual
            var periodoActual = await _periodoService.ObtenerActivoAsync();

            // Obtener tareas pendientes si está en un período activo
            List<TareaDto> tareasPendientes = new();
            List<EntregaTareaDto> entregasEntregadas = new();
            
            if (periodoActual != null && alumno.SeccionId.HasValue)
            {
                var tareas = await _tareaService.ObtenerPorAlumnoAsync(alumnoId, periodoActual.Id);
                tareasPendientes = _mapper.Map<List<TareaDto>>(tareas.Where(t => t.Activa && t.FechaEntrega > DateTime.UtcNow));
                
                // Obtener entregas realizadas
                var entregas = await _tareaService.ObtenerEntregasPorAlumnoAsync(alumnoId);
                entregasEntregadas = _mapper.Map<List<EntregaTareaDto>>(entregas);
            }

            return Ok(new
            {
                alumno = alumnoDto,
                periodoActual = _mapper.Map<PeriodoAcademicoDto>(periodoActual),
                tareasPendientes = tareasPendientes,
                entregasRealizadas = entregasEntregadas.Count(),
                tareasSinEntregar = tareasPendientes.Count(t => !entregasEntregadas.Any(e => e.TareaId == t.Id))
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener dashboard del alumno");
            return StatusCode(500, new { message = "Error al obtener el dashboard" });
        }
    }

    /// <summary>
    /// Obtiene los cursos del alumno
    /// </summary>
    [HttpGet("{alumnoId}/cursos")]
    public async Task<ActionResult<IEnumerable<CursoDto>>> ObtenerCursosAlumno(int alumnoId)
    {
        try
        {
            _logger.LogInformation($"Obteniendo cursos del alumno {alumnoId}");
            
            var alumno = await _alumnoService.ObtenerPorIdAsync(alumnoId);
            if (alumno == null)
            {
                return NotFound(new { message = "Alumno no encontrado" });
            }

            if (!alumno.SeccionId.HasValue)
            {
                return Ok(new List<CursoDto>());
            }

            // Obtener horarios de la sección del alumno
            var horarios = await _horarioService.ObtenerPorSeccionAsync(alumno.SeccionId.Value);
            
            if (!horarios.Any())
            {
                return Ok(new List<CursoDto>());
            }

            // Agregar servicios de curso que estén faltando
            return Ok(new { message = "Cursos obtenidos" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cursos del alumno");
            return StatusCode(500, new { message = "Error al obtener los cursos" });
        }
    }

    /// <summary>
    /// Actualiza el perfil del alumno
    /// </summary>
    [HttpPut("{alumnoId}")]
    public async Task<ActionResult<AlumnoDto>> ActualizarPerfil(int alumnoId, [FromBody] UpdateAlumnoDto updateDto)
    {
        try
        {
            var alumno = await _alumnoService.ObtenerPorIdAsync(alumnoId);
            if (alumno == null)
            {
                return NotFound(new { message = "Alumno no encontrado" });
            }

            // Validar que solo el alumno pueda actualizar su propio perfil
            var sessionAlumnoId = HttpContext.Session.GetInt32("AlumnoId");
            if (sessionAlumnoId != alumnoId)
            {
                return Forbid("No tienes permiso para actualizar este perfil");
            }

            _mapper.Map(updateDto, alumno);
            var actualizado = await _alumnoService.ActualizarAsync(alumno);

            if (!actualizado)
            {
                return BadRequest(new { message = "No se pudo actualizar el perfil" });
            }

            var alumnoDto = _mapper.Map<AlumnoDto>(alumno);
            return Ok(alumnoDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar el perfil del alumno");
            return StatusCode(500, new { message = "Error al actualizar el perfil" });
        }
    }
}
