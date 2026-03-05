namespace AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

public interface IPlanificacionExportService
{
    byte[] ExportarPdf(Planificacion planificacion);
    byte[] ExportarWord(Planificacion planificacion);
}
