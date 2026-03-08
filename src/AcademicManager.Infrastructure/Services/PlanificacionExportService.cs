using AcademicManager.Application.Interfaces;
using AcademicManager.Application.Services;
using AcademicManager.Domain.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OpenXmlDocument = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace AcademicManager.Infrastructure.Services;

public class PlanificacionExportService : IPlanificacionExportService
{
    public PlanificacionExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] ExportarPdf(Planificacion plan)
    {
        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(h =>
                {
                    h.Item().Background(Colors.Blue.Darken3).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Text("REPÚBLICA DOMINICANA\nMINISTERIO DE EDUCACIÓN (MINERD)")
                            .FontColor(Colors.White).FontSize(10).Bold();
                        row.RelativeItem().AlignRight().Text("PLANIFICACIÓN DE CLASE")
                            .FontColor(Colors.White).FontSize(14).Bold();
                    });
                    h.Item().PaddingTop(5).Text($"Fecha de Clase: {plan.FechaClase:dd/MM/yyyy}")
                        .FontSize(9).Italic();
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Border(1).BorderColor(Colors.Grey.Medium).Padding(8).Column(info =>
                    {
                        info.Item().Row(r =>
                        {
                            r.RelativeItem().Text(t =>
                            {
                                t.Span("Docente: ").Bold();
                                t.Span($"{plan.Docente?.Nombres} {plan.Docente?.Apellidos}");
                            });
                            r.RelativeItem().Text(t =>
                            {
                                t.Span("Curso: ").Bold();
                                t.Span($"{plan.Curso?.Nombre}");
                            });
                        });

                        info.Item().PaddingTop(4).Row(r =>
                        {
                            r.RelativeItem().Text(t =>
                            {
                                t.Span("Sección: ").Bold();
                                t.Span($"{plan.Seccion?.Nombre}");
                            });
                            r.RelativeItem().Text(t =>
                            {
                                t.Span("Periodo: ").Bold();
                                t.Span($"{plan.PeriodoAcademico?.Nombre}");
                            });
                        });

                        if (!string.IsNullOrEmpty(plan.TituloUnidad))
                        {
                            info.Item().PaddingTop(4).Row(r =>
                            {
                                r.RelativeItem().Text(t =>
                                {
                                    t.Span("Unidad: ").Bold();
                                    t.Span(plan.TituloUnidad);
                                });
                                r.RelativeItem().Text(t =>
                                {
                                    t.Span("Mes: ").Bold();
                                    t.Span(plan.Mes);
                                });
                            });
                        }
                    });

                    col.Item().PaddingTop(8);

                    RichTextExportHelper.AddPdfSection(col, "📌 Título / Tema", plan.Titulo);
                    RichTextExportHelper.AddPdfSection(col, "🎯 Situación de Aprendizaje", plan.SituacionAprendizaje);
                    RichTextExportHelper.AddPdfSection(col, "🏆 Competencias Fundamentales", plan.CompetenciasFundamentales);
                    RichTextExportHelper.AddPdfSection(col, "🎓 Competencias Específicas", plan.CompetenciasEspecificas);
                    RichTextExportHelper.AddPdfSection(col, "📚 Contenidos Conceptuales", plan.ContenidosConceptuales);
                    RichTextExportHelper.AddPdfSection(col, "📚 Contenidos Procedimentales", plan.ContenidosProcedimentales);
                    RichTextExportHelper.AddPdfSection(col, "📚 Contenidos Actitudinales", plan.ContenidosActitudinales);
                    RichTextExportHelper.AddPdfSection(col, "📊 Indicadores de Logro", plan.IndicadoresLogro);
                    RichTextExportHelper.AddPdfSection(col, "🎯 Objetivos de Aprendizaje", plan.Objetivos);
                    RichTextExportHelper.AddPdfSection(col, "📖 Desarrollo de la Clase", plan.Contenido);
                    RichTextExportHelper.AddPdfSection(col, "📐 Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
                    RichTextExportHelper.AddPdfSection(col, "📋 Metodología", plan.Metodologia);
                    RichTextExportHelper.AddPdfSection(col, "🔧 Recursos Didácticos", string.IsNullOrWhiteSpace(plan.RecursosDidacticos) ? plan.Recursos : plan.RecursosDidacticos);
                    RichTextExportHelper.AddPdfSection(col, "🔀 Ejes Transversales", plan.EjesTransversales);
                    RichTextExportHelper.AddPdfSection(col, "✅ Evaluación", plan.Evaluacion);
                    RichTextExportHelper.AddPdfSection(col, "📝 Actividades de Evaluación", plan.ActividadesEvaluacion);
                    RichTextExportHelper.AddPdfSection(col, "💬 Observaciones", plan.Observaciones);
                });

                page.Footer().BorderTop(1).BorderColor(Colors.Grey.Medium).PaddingTop(5).Row(footer =>
                {
                    footer.RelativeItem().Text($"Estado: {plan.Estado}").FontSize(8);
                    footer.RelativeItem().AlignCenter().Text(x =>
                    {
                        x.DefaultTextStyle(ts => ts.FontSize(8));
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
                    footer.RelativeItem().AlignRight().Text($"Generado: {DateTime.Now:dd/MM/yyyy}").FontSize(8);
                });
            });
        });

        using var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        return memoryStream.ToArray();
    }

    public byte[] ExportarWord(Planificacion plan)
    {
        using var memoryStream = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new OpenXmlDocument(new Body());
            var body = mainPart.Document.Body!;

            body.Append(RichTextExportHelper.CreateWordParagraph("REPÚBLICA DOMINICANA - MINISTERIO DE EDUCACIÓN (MINERD)", true, "24"));
            body.Append(RichTextExportHelper.CreateWordParagraph("PLANIFICACIÓN DE CLASE", true, "28"));
            body.Append(RichTextExportHelper.CreateWordParagraph(string.Empty));

            body.Append(RichTextExportHelper.CreateWordParagraph($"Docente: {plan.Docente?.Nombres} {plan.Docente?.Apellidos}"));
            body.Append(RichTextExportHelper.CreateWordParagraph($"Curso: {plan.Curso?.Nombre} — Sección: {plan.Seccion?.Nombre}"));
            body.Append(RichTextExportHelper.CreateWordParagraph($"Periodo: {plan.PeriodoAcademico?.Nombre}"));
            body.Append(RichTextExportHelper.CreateWordParagraph($"Fecha de Clase: {plan.FechaClase:dd/MM/yyyy}"));
            if (!string.IsNullOrWhiteSpace(plan.TituloUnidad))
            {
                body.Append(RichTextExportHelper.CreateWordParagraph($"Unidad: {plan.TituloUnidad} — Mes: {plan.Mes}"));
            }

            body.Append(RichTextExportHelper.CreateWordParagraph(string.Empty));

            RichTextExportHelper.AddWordSection(wordDocument, body, "Título / Tema", plan.Titulo);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Situación de Aprendizaje", plan.SituacionAprendizaje);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Competencias Fundamentales", plan.CompetenciasFundamentales);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Competencias Específicas", plan.CompetenciasEspecificas);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Contenidos Conceptuales", plan.ContenidosConceptuales);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Contenidos Procedimentales", plan.ContenidosProcedimentales);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Contenidos Actitudinales", plan.ContenidosActitudinales);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Indicadores de Logro", plan.IndicadoresLogro);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Objetivos de Aprendizaje", plan.Objetivos);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Desarrollo de la Clase", plan.Contenido);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Metodología", plan.Metodologia);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Recursos Didácticos", string.IsNullOrWhiteSpace(plan.RecursosDidacticos) ? plan.Recursos : plan.RecursosDidacticos);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Ejes Transversales", plan.EjesTransversales);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Evaluación", plan.Evaluacion);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Actividades de Evaluación", plan.ActividadesEvaluacion);
            RichTextExportHelper.AddWordSection(wordDocument, body, "Observaciones", plan.Observaciones);

            body.Append(RichTextExportHelper.CreateWordParagraph(string.Empty));
            body.Append(RichTextExportHelper.CreateWordParagraph($"Estado: {plan.Estado} | Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"));

            mainPart.Document.Save();
        }

        return memoryStream.ToArray();
    }
}
