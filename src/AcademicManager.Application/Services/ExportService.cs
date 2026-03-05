using AcademicManager.Domain.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AcademicManager.Application.Services;

public class ExportService
{
    public ExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ==========================================================
    // PDF GENERATION
    // ==========================================================

    public byte[] GenerarPdfPlanAnual(Planificacion plan)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text("Planificación Anual MINERD").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Item().Text($"Título: {plan.Titulo}").Bold().FontSize(14);
                    x.Item().Text($"Docente: {plan.Docente?.Nombres} {plan.Docente?.Apellidos}");
                    x.Item().Text($"Curso: {plan.Curso?.Nombre} | Año Académico: {plan.AnoAcademico}");
                    
                    x.Item().PaddingTop(10).Text("Situación de Aprendizaje").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.SituacionAprendizaje) ? plan.SituacionAprendizaje : "N/A");

                    x.Item().PaddingTop(10).Text("Competencias Fundamentales").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.CompetenciasFundamentales) ? plan.CompetenciasFundamentales : "N/A");

                    x.Item().PaddingTop(10).Text("Competencias Específicas").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.CompetenciasEspecificas) ? plan.CompetenciasEspecificas : "N/A");

                    x.Item().PaddingTop(10).Text("Contenidos Conceptuales").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.ContenidosConceptuales) ? plan.ContenidosConceptuales : "N/A");

                    x.Item().PaddingTop(10).Text("Estrategias de Enseñanza").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.EstrategiasEnsenanza) ? plan.EstrategiasEnsenanza : "N/A");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerarPdfPlanMensual(PlanificacionMensual plan)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Text($"Planificación Mensual - {plan.Mes}").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Item().Text($"Unidad: {plan.TituloUnidad}").Bold().FontSize(14);
                    
                    x.Item().PaddingTop(10).Text("Situación de Aprendizaje").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.SituacionAprendizaje) ? plan.SituacionAprendizaje : "N/A");

                    x.Item().PaddingTop(10).Text("Competencias Generales y Específicas").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.CompetenciasEspecificas) ? plan.CompetenciasEspecificas : "N/A");

                    x.Item().PaddingTop(10).Text("Contenidos Generales").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.ContenidosConceptuales) ? plan.ContenidosConceptuales : "N/A");

                    x.Item().PaddingTop(10).Text("Indicadores de Logro").Bold();
                    x.Item().Text(!string.IsNullOrEmpty(plan.IndicadoresLogro) ? plan.IndicadoresLogro : "N/A");
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerarPdfPlanDiario(PlanificacionDiaria plan)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                
                page.Header().Text($"Planificación Diaria - {plan.Fecha.ToShortDateString()}").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Item().Text($"Intención Pedagógica:").Bold().FontSize(12);
                    x.Item().Text(plan.IntencionPedagogica);
                    
                    x.Item().PaddingTop(10).Text($"Momentos de la Clase").Bold().FontSize(14);
                    
                    x.Item().PaddingTop(5).Text($"Inicio ({plan.TiempoInicioMinutos} min)").Bold();
                    x.Item().Text(plan.ActividadesInicio);

                    x.Item().PaddingTop(5).Text($"Desarrollo ({plan.TiempoDesarrolloMinutos} min)").Bold();
                    x.Item().Text(plan.ActividadesDesarrollo);

                    x.Item().PaddingTop(5).Text($"Cierre ({plan.TiempoCierreMinutos} min)").Bold();
                    x.Item().Text(plan.ActividadesCierre);

                    x.Item().PaddingTop(10).Text("Recursos:").Bold();
                    x.Item().Text(plan.Recursos);
                });
            });
        }).GeneratePdf();
    }

    // ==========================================================
    // WORD GENERATION
    // ==========================================================

    public byte[] GenerarWordPlanAnual(Planificacion plan)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            MainDocumentPart mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            Body body = new Body();
            
            AddParagraph(body, "Planificación Anual MINERD", true, 24);
            AddParagraph(body, $"Título: {plan.Titulo}");
            AddParagraph(body, $"Docente: {plan.Docente?.Nombres} {plan.Docente?.Apellidos}");
            AddParagraph(body, $"Curso: {plan.Curso?.Nombre} | Año Académico: {plan.AnoAcademico}");
            
            AddParagraph(body, "Situación de Aprendizaje", true, 14);
            AddParagraph(body, plan.SituacionAprendizaje ?? "N/A");
            
            AddParagraph(body, "Competencias Específicas", true, 14);
            AddParagraph(body, plan.CompetenciasEspecificas ?? "N/A");
            
            AddParagraph(body, "Contenidos Conceptuales", true, 14);
            AddParagraph(body, plan.ContenidosConceptuales ?? "N/A");

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }
        return stream.ToArray();
    }

    public byte[] GenerarWordPlanMensual(PlanificacionMensual plan)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            MainDocumentPart mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            Body body = new Body();
            
            AddParagraph(body, $"Planificación Mensual - {plan.Mes}", true, 24);
            AddParagraph(body, $"Unidad: {plan.TituloUnidad}");
            
            AddParagraph(body, "Situación de Aprendizaje", true, 14);
            AddParagraph(body, plan.SituacionAprendizaje ?? "N/A");
            
            AddParagraph(body, "Competencias Específicas", true, 14);
            AddParagraph(body, plan.CompetenciasEspecificas ?? "N/A");

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }
        return stream.ToArray();
    }

    public byte[] GenerarWordPlanDiario(PlanificacionDiaria plan)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            MainDocumentPart mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            Body body = new Body();
            
            AddParagraph(body, $"Planificación Diaria - {plan.Fecha.ToShortDateString()}", true, 24);
            AddParagraph(body, "Intención Pedagógica:", true, 14);
            AddParagraph(body, plan.IntencionPedagogica ?? "N/A");
            
            AddParagraph(body, "Inicio:", true, 14);
            AddParagraph(body, plan.ActividadesInicio ?? "N/A");

            AddParagraph(body, "Desarrollo:", true, 14);
            AddParagraph(body, plan.ActividadesDesarrollo ?? "N/A");
            
            AddParagraph(body, "Cierre:", true, 14);
            AddParagraph(body, plan.ActividadesCierre ?? "N/A");

            mainPart.Document.Append(body);
            mainPart.Document.Save();
        }
        return stream.ToArray();
    }

    private void AddParagraph(Body body, string text, bool isBold = false, int fontSize = 0)
    {
        var run = new Run(new Text(text));
        if (isBold || fontSize > 0)
        {
            var runProps = new RunProperties();
            if (isBold) runProps.Append(new Bold());
            if (fontSize > 0) runProps.Append(new FontSize { Val = (fontSize * 2).ToString() });
            run.PrependChild(runProps);
        }

        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var p = new Paragraph();

        for (int i = 0; i < lines.Length; i++)
        {
            var lineRun = new Run();
            var runProps2 = new RunProperties();
            if (isBold) runProps2.Append(new Bold());
            if (fontSize > 0) runProps2.Append(new FontSize { Val = (fontSize * 2).ToString() });
            lineRun.PrependChild(runProps2);

            lineRun.Append(new Text(lines[i]));
            if (i < lines.Length - 1) lineRun.Append(new Break());
            
            p.Append(lineRun);
        }

        if (lines.Length == 0) p.Append(new Run(new Text(" ")));

        body.Append(p);
    }
}
