using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class SolicitudRegistroService
{
    private readonly ISolicitudRegistroRepository _repository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly UsuarioService _usuarioService;

    public SolicitudRegistroService(
        ISolicitudRegistroRepository repository,
        IUsuarioRepository usuarioRepository,
        UsuarioService usuarioService)
    {
        _repository = repository;
        _usuarioRepository = usuarioRepository;
        _usuarioService = usuarioService;
    }

    public Task<IEnumerable<SolicitudRegistro>> ObtenerTodosAsync() => _repository.GetAllAsync();
    
    public Task<IEnumerable<SolicitudRegistro>> ObtenerPendientesAsync() => _repository.GetByEstadoAsync("Pendiente");

    public async Task<int> CrearSolicitudAsync(SolicitudRegistro solicitud, string password)
    {
        // El password se hashea antes de guardarse en la solicitud para que el admin no lo vea plano
        solicitud.PasswordHash = AuthService.HashPassword(password);
        solicitud.Estado = "Pendiente";
        solicitud.FechaSolicitud = DateTime.Now;
        
        return await _repository.CreateAsync(solicitud);
    }

    public async Task<bool> ProcesarSolicitudAsync(int id, bool aprobada)
    {
        var solicitud = await _repository.GetByIdAsync(id);
        if (solicitud == null || solicitud.Estado != "Pendiente") return false;

        if (aprobada)
        {
            // Crear el usuario real
            var nuevoUsuario = new Usuario
            {
                NombreUsuario = solicitud.NombreUsuario,
                Email = solicitud.Email,
                PasswordHash = solicitud.PasswordHash,
                Rol = solicitud.RolSolicitado,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            var usuarioId = await _usuarioRepository.CreateAsync(nuevoUsuario);

            if (string.Equals(solicitud.RolSolicitado, "Alumno", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(solicitud.RolSolicitado, "Docente", StringComparison.OrdinalIgnoreCase))
            {
                await _usuarioService.ConvertirRolAsync(usuarioId, solicitud.RolSolicitado);
            }
        }

        await _repository.UpdateEstadoAsync(id, aprobada ? "Aprobado" : "Rechazado");
        return true;
    }
    
    public Task<bool> EliminarAsync(int id) => _repository.DeleteAsync(id);
}
