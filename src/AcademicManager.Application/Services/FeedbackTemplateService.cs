using AcademicManager.Application.DTOs;
using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;

namespace AcademicManager.Application.Services;

/// <summary>
/// Servicio para gestionar plantillas reutilizables de retroalimentación.
/// Permite a docentes crear y usar comentarios frecuentes.
/// </summary>
public class FeedbackTemplateService
{
    private readonly IFeedbackTemplateRepository _templateRepository;
    private readonly IEntregaTareaRepository _entregaTareaRepository;

    public FeedbackTemplateService(
        IFeedbackTemplateRepository templateRepository,
        IEntregaTareaRepository entregaTareaRepository)
    {
        _templateRepository = templateRepository;
        _entregaTareaRepository = entregaTareaRepository;
    }

    /// <summary>
    /// Obtiene plantillas de retroalimentación por asignatura.
    /// </summary>
    public async Task<List<FeedbackTemplateDto>> GetTemplatesBySubjectAsync(string materia)
    {
        var templates = (await _templateRepository.GetByMateriaAsync(materia))
            .Where(t => t.Activa)
            .OrderBy(t => t.Orden)
            .ToList();

        return templates.Select(t => new FeedbackTemplateDto
        {
            Id = t.Id,
            DocenteId = t.DocenteId,
            Titulo = t.Titulo,
            Contenido = t.Contenido,
            Materia = t.Materia,
            Orden = t.Orden,
            Activa = t.Activa,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Obtiene todas las plantillas de un docente.
    /// </summary>
    public async Task<List<FeedbackTemplateDto>> GetTemplatesByDocenteAsync(int docenteId)
    {
        var templates = (await _templateRepository.GetByDocenteIdAsync(docenteId))
            .OrderBy(t => t.Orden)
            .ToList();

        return templates.Select(t => new FeedbackTemplateDto
        {
            Id = t.Id,
            DocenteId = t.DocenteId,
            Titulo = t.Titulo,
            Contenido = t.Contenido,
            Materia = t.Materia,
            Orden = t.Orden,
            Activa = t.Activa,
            CreatedAt = t.CreatedAt
        }).ToList();
    }

    /// <summary>
    /// Crea una nueva plantilla de retroalimentación.
    /// </summary>
    public async Task<(bool Success, string Message, int? TemplateId)> CreateTemplateAsync(
        int docenteId,
        CreateFeedbackTemplateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Titulo) || string.IsNullOrWhiteSpace(dto.Contenido))
            return (false, "Título y contenido son requeridos.", null);

        var template = new FeedbackTemplate
        {
            DocenteId = docenteId,
            Titulo = dto.Titulo,
            Contenido = dto.Contenido,
            Materia = dto.Materia,
            Orden = dto.Orden,
            Activa = true,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _templateRepository.CreateAsync(template);

        return (true, "Plantilla creada exitosamente.", id);
    }

    /// <summary>
    /// Actualiza una plantilla existente.
    /// </summary>
    public async Task<(bool Success, string Message)> UpdateTemplateAsync(
        int templateId,
        int docenteId,
        UpdateFeedbackTemplateDto dto)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
            return (false, "Plantilla no encontrada.");

        if (template.DocenteId != docenteId)
            return (false, "No tienes permiso para actualizar esta plantilla.");

        template.Titulo = dto.Titulo;
        template.Contenido = dto.Contenido;
        template.Materia = dto.Materia;
        template.Orden = dto.Orden;
        template.Activa = dto.Activa;
        template.UpdatedAt = DateTime.UtcNow;

        var success = await _templateRepository.UpdateAsync(template);

        return (success, success ? "Plantilla actualizada exitosamente." : "Error al actualizar plantilla.");
    }

    /// <summary>
    /// Elimina una plantilla.
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteTemplateAsync(int templateId, int docenteId)
    {
        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
            return (false, "Plantilla no encontrada.");

        if (template.DocenteId != docenteId)
            return (false, "No tienes permiso para eliminar esta plantilla.");

        var success = await _templateRepository.DeleteAsync(templateId);

        return (success, success ? "Plantilla eliminada exitosamente." : "Error al eliminar plantilla.");
    }

    /// <summary>
    /// Añade una plantilla a la retroalimentación de una entrega.
    /// </summary>
    public async Task<(bool Success, string Message)> AppendTemplateToFeedbackAsync(
        int entregaId,
        int templateId)
    {
        var entrega = await _entregaTareaRepository.GetByIdAsync(entregaId);
        if (entrega == null)
            return (false, "Entrega no encontrada.");

        var template = await _templateRepository.GetByIdAsync(templateId);
        if (template == null)
            return (false, "Plantilla no encontrada.");

        if (!template.Activa)
            return (false, "Plantilla está inactiva.");

        var retroalimentacionAnterior = entrega.Retroalimentacion ?? string.Empty;
        var retroalimentacionNueva = $"{retroalimentacionAnterior}\n{template.Contenido}".Trim();

        entrega.Retroalimentacion = retroalimentacionNueva;

        var success = await _entregaTareaRepository.UpdateAsync(entrega);

        return (success, success ? "Plantilla añadida a retroalimentación." : "Error al añadir plantilla.");
    }

    /// <summary>
    /// Obtiene análisis de patrones de retroalimentación.
    /// </summary>
    public async Task<List<string>> GetRecentFeedbackPatternsAsync(int docenteId, int ultimasNTareas = 10)
    {
        // Análisis simplificado de palabras clave frecuentes
        var templates = await GetTemplatesByDocenteAsync(docenteId);

        return templates
            .OrderByDescending(t => t.CreatedAt)
            .Take(ultimasNTareas)
            .Select(t => t.Titulo)
            .ToList();
    }
}
