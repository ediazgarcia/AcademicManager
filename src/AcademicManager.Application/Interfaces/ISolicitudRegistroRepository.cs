using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Interfaces;

public interface ISolicitudRegistroRepository
{
    Task<SolicitudRegistro?> GetByIdAsync(int id);
    Task<IEnumerable<SolicitudRegistro>> GetAllAsync();
    Task<IEnumerable<SolicitudRegistro>> GetByEstadoAsync(string estado);
    Task<int> CreateAsync(SolicitudRegistro solicitud);
    Task<bool> UpdateEstadoAsync(int id, string estado);
    Task<bool> DeleteAsync(int id);
}
