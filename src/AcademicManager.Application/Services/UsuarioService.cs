using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace AcademicManager.Application.Services;

public class UsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
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
        // Don't update password here
        
        return await _usuarioRepository.UpdateAsync(existing);
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
}
