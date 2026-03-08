using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

public class UsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAlumnoRepository _alumnoRepository;
    private readonly IDocenteRepository _docenteRepository;
    private readonly IMatriculaCursoRepository _matriculaCursoRepository;
    private readonly IDocenteCursoRepository _docenteCursoRepository;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IAlumnoRepository alumnoRepository,
        IDocenteRepository docenteRepository,
        IMatriculaCursoRepository matriculaCursoRepository,
        IDocenteCursoRepository docenteCursoRepository)
    {
        _usuarioRepository = usuarioRepository;
        _alumnoRepository = alumnoRepository;
        _docenteRepository = docenteRepository;
        _matriculaCursoRepository = matriculaCursoRepository;
        _docenteCursoRepository = docenteCursoRepository;
    }

    public async Task<IEnumerable<Usuario>> ObtenerTodosAsync()
    {
        return await _usuarioRepository.GetAllAsync();
    }

    public async Task<Usuario?> ObtenerPorIdAsync(int id)
    {
        return await _usuarioRepository.GetByIdAsync(id);
    }

    public async Task<int> CrearAsync(Usuario usuario, string password) // Admin forced creation
    {
        usuario.PasswordHash = AuthService.HashPassword(password);
        return await _usuarioRepository.CreateAsync(usuario);
    }

    public async Task<bool> ActualizarAsync(Usuario usuario) // Updates fields except password
    {
        // Fetch current user logic? 
        // For simplicity, just update fields. 
        // But Repository.UpdateAsync expects full object.
        var existing = await _usuarioRepository.GetByIdAsync(usuario.Id);
        if (existing == null) return false;

        existing.Rol = usuario.Rol;
        existing.Email = usuario.Email;
        existing.Activo = usuario.Activo;
        existing.AlumnoId = usuario.AlumnoId;
        existing.DocenteId = usuario.DocenteId;
        // Don't update password here
        
        return await _usuarioRepository.UpdateAsync(existing);
    }

    public async Task<bool> ConvertirRolAsync(int usuarioId, string nuevoRol)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
        if (usuario == null)
        {
            return false;
        }

        if (string.Equals(usuario.Rol, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("No se permite convertir usuarios administradores.");
        }

        if (!string.Equals(nuevoRol, "Alumno", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(nuevoRol, "Docente", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Solo se permite convertir a rol Alumno o Docente.");
        }

        if (string.Equals(nuevoRol, "Alumno", StringComparison.OrdinalIgnoreCase))
        {
            await RemoveDocenteLinkAsync(usuario);

            if (!usuario.AlumnoId.HasValue)
            {
                var alumno = await BuildAlumnoFromUsuarioAsync(usuario);
                usuario.AlumnoId = await _alumnoRepository.CreateAsync(alumno);
            }

            usuario.Rol = "Alumno";
        }
        else
        {
            await RemoveAlumnoLinkAsync(usuario);

            if (!usuario.DocenteId.HasValue)
            {
                var docente = await BuildDocenteFromUsuarioAsync(usuario);
                usuario.DocenteId = await _docenteRepository.CreateAsync(docente);
            }

            usuario.Rol = "Docente";
        }

        return await _usuarioRepository.UpdateAsync(usuario);
    }
    
    public async Task<bool> CambiarPasswordAdminAsync(int id, string newPassword)
    {
        var existing = await _usuarioRepository.GetByIdAsync(id);
        if (existing == null) return false;
        
        existing.PasswordHash = AuthService.HashPassword(newPassword);
        return await _usuarioRepository.UpdateAsync(existing);
    }

    public async Task<bool> EliminarAsync(int id)
    {
        return await _usuarioRepository.DeleteAsync(id);
    }

    private async Task<Alumno> BuildAlumnoFromUsuarioAsync(Usuario usuario)
    {
        var (nombres, apellidos) = SplitHumanName(usuario.NombreUsuario, usuario.Email);

        return new Alumno
        {
            Codigo = await GenerateAlumnoCodeAsync(),
            Nombres = nombres,
            Apellidos = apellidos,
            Email = usuario.Email ?? string.Empty,
            FechaNacimiento = DateTime.UtcNow.AddYears(-12),
            Genero = "Otro",
            Direccion = string.Empty,
            Telefono = string.Empty,
            NombreApoderado = string.Empty,
            TelefonoApoderado = string.Empty,
            Activo = usuario.Activo,
            FechaRegistro = DateTime.UtcNow
        };
    }

    private async Task<Docente> BuildDocenteFromUsuarioAsync(Usuario usuario)
    {
        var (nombres, apellidos) = SplitHumanName(usuario.NombreUsuario, usuario.Email);

        return new Docente
        {
            Codigo = await GenerateDocenteCodeAsync(),
            Nombres = nombres,
            Apellidos = apellidos,
            Email = usuario.Email ?? string.Empty,
            FechaNacimiento = DateTime.UtcNow.AddYears(-25),
            Genero = "Otro",
            Direccion = string.Empty,
            Telefono = string.Empty,
            Especialidad = "Por asignar",
            GradoAcademico = "Por definir",
            FechaContratacion = DateTime.UtcNow,
            Activo = usuario.Activo,
            FechaRegistro = DateTime.UtcNow
        };
    }

    private async Task<string> GenerateAlumnoCodeAsync()
    {
        while (true)
        {
            var candidate = $"ALU{DateTime.UtcNow:yyMMddHHmm}{Random.Shared.Next(1000, 10000)}";
            var existing = await _alumnoRepository.GetByCodigoAsync(candidate);
            if (existing == null)
            {
                return candidate;
            }
        }
    }

    private async Task<string> GenerateDocenteCodeAsync()
    {
        while (true)
        {
            var candidate = $"DOC{DateTime.UtcNow:yyMMddHHmm}{Random.Shared.Next(1000, 10000)}";
            var existing = await _docenteRepository.GetByCodigoAsync(candidate);
            if (existing == null)
            {
                return candidate;
            }
        }
    }

    private static (string nombres, string apellidos) SplitHumanName(string? username, string? email)
    {
        var source = string.IsNullOrWhiteSpace(username)
            ? (email?.Split('@').FirstOrDefault() ?? "Usuario")
            : username;

        var tokens = source
            .Replace(".", " ", StringComparison.Ordinal)
            .Replace("_", " ", StringComparison.Ordinal)
            .Replace("-", " ", StringComparison.Ordinal)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(ToTitleCase)
            .ToList();

        if (tokens.Count == 0)
        {
            return ("Usuario", "Pendiente");
        }

        if (tokens.Count == 1)
        {
            return (tokens[0], "Pendiente");
        }

        return (tokens[0], string.Join(' ', tokens.Skip(1)));
    }

    private static string ToTitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (value.Length == 1)
        {
            return value.ToUpperInvariant();
        }

        return char.ToUpperInvariant(value[0]) + value[1..].ToLowerInvariant();
    }

    private async Task RemoveAlumnoLinkAsync(Usuario usuario)
    {
        if (!usuario.AlumnoId.HasValue)
        {
            return;
        }

        var alumnoId = usuario.AlumnoId.Value;
        var alumno = await _alumnoRepository.GetByIdAsync(alumnoId);
        if (alumno != null)
        {
            await _matriculaCursoRepository.DeleteByAlumnoAsync(alumnoId);
            var deleted = await _alumnoRepository.DeleteAsync(alumnoId);
            if (!deleted)
            {
                throw new InvalidOperationException("No se pudo eliminar el registro de alumno asociado al usuario.");
            }
        }

        usuario.AlumnoId = null;
    }

    private async Task RemoveDocenteLinkAsync(Usuario usuario)
    {
        if (!usuario.DocenteId.HasValue)
        {
            return;
        }

        var docenteId = usuario.DocenteId.Value;
        var docente = await _docenteRepository.GetByIdAsync(docenteId);
        if (docente != null)
        {
            await _docenteCursoRepository.DeleteByDocenteAsync(docenteId);
            var deleted = await _docenteRepository.DeleteAsync(docenteId);
            if (!deleted)
            {
                throw new InvalidOperationException("No se pudo eliminar el registro de docente asociado al usuario.");
            }
        }

        usuario.DocenteId = null;
    }
}
