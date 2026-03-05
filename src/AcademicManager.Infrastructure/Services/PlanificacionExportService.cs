using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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

                // Header
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

                // Content
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Info Box
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

                    // Título / Tema
                    AddSection(col, "📌 Título / Tema", plan.Titulo);

                    // Situación de Aprendizaje
                    if (!string.IsNullOrWhiteSpace(plan.SituacionAprendizaje))
                        AddSection(col, "🎯 Situación de Aprendizaje", plan.SituacionAprendizaje);

                    // Competencias
                    if (!string.IsNullOrWhiteSpace(plan.CompetenciasFundamentales))
                        AddSection(col, "🏆 Competencias Fundamentales", plan.CompetenciasFundamentales);
                    if (!string.IsNullOrWhiteSpace(plan.CompetenciasEspecificas))
                        AddSection(col, "🎓 Competencias Específicas", plan.CompetenciasEspecificas);

                    // Contenidos
                    if (!string.IsNullOrWhiteSpace(plan.ContenidosConceptuales) ||
                        !string.IsNullOrWhiteSpace(plan.ContenidosProcedimentales) ||
                        !string.IsNullOrWhiteSpace(plan.ContenidosActitudinales))
                    {
                        col.Item().PaddingTop(6).Text("📚 Contenidos").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                        if (!string.IsNullOrWhiteSpace(plan.ContenidosConceptuales))
                            col.Item().PaddingLeft(10).Text($"• Conceptuales: {plan.ContenidosConceptuales}");
                        if (!string.IsNullOrWhiteSpace(plan.ContenidosProcedimentales))
                            col.Item().PaddingLeft(10).Text($"• Procedimentales: {plan.ContenidosProcedimentales}");
                        if (!string.IsNullOrWhiteSpace(plan.ContenidosActitudinales))
                            col.Item().PaddingLeft(10).Text($"• Actitudinales: {plan.ContenidosActitudinales}");
                    }

                    // Indicadores de Logro
                    if (!string.IsNullOrWhiteSpace(plan.IndicadoresLogro))
                        AddSection(col, "📊 Indicadores de Logro", plan.IndicadoresLogro);

                    // Objetivos
                    if (!string.IsNullOrWhiteSpace(plan.Objetivos))
                        AddSection(col, "🎯 Objetivos de Aprendizaje", plan.Objetivos);

                    // Contenido/Desarrollo general
                    if (!string.IsNullOrWhiteSpace(plan.Contenido))
                        AddSection(col, "📖 Desarrollo de la Clase", plan.Contenido);

                    // Estrategias de Enseñanza
                    if (!string.IsNullOrWhiteSpace(plan.EstrategiasEnsenanza))
                        AddSection(col, "📐 Estrategias de Enseñanza", plan.EstrategiasEnsenanza);

                    // Metodología
                    if (!string.IsNullOrWhiteSpace(plan.Metodologia))
                        AddSection(col, "📋 Metodología", plan.Metodologia);

                    // Recursos
                    if (!string.IsNullOrWhiteSpace(plan.Recursos))
                        AddSection(col, "🔧 Recursos Didácticos", plan.Recursos);

                    // Ejes Transversales
                    if (!string.IsNullOrWhiteSpace(plan.EjesTransversales))
                        AddSection(col, "🔀 Ejes Transversales", plan.EjesTransversales);

                    // Evaluación
                    if (!string.IsNullOrWhiteSpace(plan.Evaluacion))
                        AddSection(col, "✅ Evaluación", plan.Evaluacion);

                    // Actividades de Evaluación
                    if (!string.IsNullOrWhiteSpace(plan.ActividadesEvaluacion))
                        AddSection(col, "📝 Actividades de Evaluación", plan.ActividadesEvaluacion);

                    // Observaciones
                    if (!string.IsNullOrWhiteSpace(plan.Observaciones))
                        AddSection(col, "💬 Observaciones", plan.Observaciones);
                });

                // Footer
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

    private void AddSection(ColumnDescriptor col, string title, string content)
    {
        col.Item().PaddingTop(6).Text(title).Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
        col.Item().PaddingLeft(10).PaddingBottom(2).Text(content ?? "");
    }

    public byte[] ExportarWord(Planificacion plan)
    {
        using var memoryStream = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(new Body());
            var body = mainPart.Document.Body!;

            // Header
            body.Append(CreateParagraph("REPÚBLICA DOMINICANA - MINISTERIO DE EDUCACIÓN (MINERD)", true, "24"));
            body.Append(CreateParagraph("PLANIFICACIÓN DE CLASE", true, "28"));
            body.Append(CreateParagraph(""));

            // Info
            body.Append(CreateParagraph($"Docente: {plan.Docente?.Nombres} {plan.Docente?.Apellidos}"));
            body.Append(CreateParagraph($"Curso: {plan.Curso?.Nombre} — Sección: {plan.Seccion?.Nombre}"));
            body.Append(CreateParagraph($"Periodo: {plan.PeriodoAcademico?.Nombre}"));
            body.Append(CreateParagraph($"Fecha de Clase: {plan.FechaClase:dd/MM/yyyy}"));

            if (!string.IsNullOrWhiteSpace(plan.TituloUnidad))
                body.Append(CreateParagraph($"Unidad: {plan.TituloUnidad} — Mes: {plan.Mes}"));

            body.Append(CreateParagraph(""));

            // Secciones
            AddWordSection(body, "Título / Tema", plan.Titulo);

            if (!string.IsNullOrWhiteSpace(plan.SituacionAprendizaje))
                AddWordSection(body, "Situación de Aprendizaje", plan.SituacionAprendizaje);

            if (!string.IsNullOrWhiteSpace(plan.CompetenciasFundamentales))
                AddWordSection(body, "Competencias Fundamentales", plan.CompetenciasFundamentales);
            if (!string.IsNullOrWhiteSpace(plan.CompetenciasEspecificas))
                AddWordSection(body, "Competencias Específicas", plan.CompetenciasEspecificas);

            if (!string.IsNullOrWhiteSpace(plan.ContenidosConceptuales) ||
                !string.IsNullOrWhiteSpace(plan.ContenidosProcedimentales) ||
                !string.IsNullOrWhiteSpace(plan.ContenidosActitudinales))
            {
                body.Append(CreateParagraph("Contenidos", true));
                if (!string.IsNullOrWhiteSpace(plan.ContenidosConceptuales))
                    body.Append(CreateParagraph($"  • Conceptuales: {plan.ContenidosConceptuales}"));
                if (!string.IsNullOrWhiteSpace(plan.ContenidosProcedimentales))
                    body.Append(CreateParagraph($"  • Procedimentales: {plan.ContenidosProcedimentales}"));
                if (!string.IsNullOrWhiteSpace(plan.ContenidosActitudinales))
                    body.Append(CreateParagraph($"  • Actitudinales: {plan.ContenidosActitudinales}"));
                body.Append(CreateParagraph(""));
            }

            if (!string.IsNullOrWhiteSpace(plan.IndicadoresLogro))
                AddWordSection(body, "Indicadores de Logro", plan.IndicadoresLogro);
            if (!string.IsNullOrWhiteSpace(plan.Objetivos))
                AddWordSection(body, "Objetivos de Aprendizaje", plan.Objetivos);
            if (!string.IsNullOrWhiteSpace(plan.Contenido))
                AddWordSection(body, "Desarrollo de la Clase", plan.Contenido);
            if (!string.IsNullOrWhiteSpace(plan.EstrategiasEnsenanza))
                AddWordSection(body, "Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
            if (!string.IsNullOrWhiteSpace(plan.Metodologia))
                AddWordSection(body, "Metodología", plan.Metodologia);
            if (!string.IsNullOrWhiteSpace(plan.Recursos))
                AddWordSection(body, "Recursos Didácticos", plan.Recursos);
            if (!string.IsNullOrWhiteSpace(plan.EjesTransversales))
                AddWordSection(body, "Ejes Transversales", plan.EjesTransversales);
            if (!string.IsNullOrWhiteSpace(plan.Evaluacion))
                AddWordSection(body, "Evaluación", plan.Evaluacion);
            if (!string.IsNullOrWhiteSpace(plan.ActividadesEvaluacion))
                AddWordSection(body, "Actividades de Evaluación", plan.ActividadesEvaluacion);
            if (!string.IsNullOrWhiteSpace(plan.Observaciones))
                AddWordSection(body, "Observaciones", plan.Observaciones);

            body.Append(CreateParagraph(""));
            body.Append(CreateParagraph($"Estado: {plan.Estado} | Generado: {DateTime.Now:dd/MM/yyyy HH:mm}"));

            mainPart.Document.Save();
        }
        return memoryStream.ToArray();
    }

    private void AddWordSection(Body body, string title, string content)
    {
        body.Append(CreateParagraph(title, true));
        body.Append(CreateParagraph(content));
        body.Append(CreateParagraph(""));
    }

    private Paragraph CreateParagraph(string text, bool isBold = false, string fontSize = "22")
    {
        var runProperties = new RunProperties(new FontSize() { Val = fontSize });
        if (isBold) runProperties.Append(new Bold());

        var run = new Run();
        run.Append(runProperties);
        run.Append(new Text(text ?? ""));

        var paragraph = new Paragraph();
        paragraph.Append(run);
        return paragraph;
    }
}
