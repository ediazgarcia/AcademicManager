using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<int> CreateAsync(Usuario usuario);
    Task<bool> UpdateAsync(Usuario usuario);
    Task<bool> DeleteAsync(int id);
    Task<bool> UpdateUltimoAccesoAsync(int id);
}
