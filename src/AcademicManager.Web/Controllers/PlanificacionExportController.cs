using AcademicManager.Application.Interfaces;
using AcademicManager.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AcademicManager.Web.Controllers;

[ApiController]
[Route("api/export/planificacion")]
public class PlanificacionExportController : ControllerBase
{
    private readonly IPlanificacionExportService _exportService;
    private readonly PlanificacionService _planificacionService;

    public PlanificacionExportController(
        IPlanificacionExportService exportService,
        PlanificacionService planificacionService)
    {
        _exportService = exportService;
        _planificacionService = planificacionService;
    }

    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> ExportarPdf(int id)
    {
        var plan = await _planificacionService.ObtenerPorIdAsync(id);
        if (plan == null) return NotFound();

        var bytes = _exportService.ExportarPdf(plan);
        var fileName = !string.IsNullOrWhiteSpace(plan.TituloUnidad)
            ? $"Planificacion_{plan.TituloUnidad}.pdf"
            : $"Planificacion_{plan.Titulo}.pdf";

        return File(bytes, "application/pdf", fileName);
    }

    [HttpGet("{id}/word")]
    public async Task<IActionResult> ExportarWord(int id)
    {
        var plan = await _planificacionService.ObtenerPorIdAsync(id);
        if (plan == null) return NotFound();

        var bytes = _exportService.ExportarWord(plan);
        var fileName = !string.IsNullOrWhiteSpace(plan.TituloUnidad)
            ? $"Planificacion_{plan.TituloUnidad}.docx"
            : $"Planificacion_{plan.Titulo}.docx";

        return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", fileName);
    }
}
