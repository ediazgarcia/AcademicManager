using AcademicManager.Domain.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OpenXmlDocument = DocumentFormat.OpenXml.Wordprocessing.Document;

namespace AcademicManager.Application.Services;

public class ExportService
{
    public ExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

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

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    col.Item().Text($"Título: {plan.Titulo}").Bold().FontSize(14);
                    col.Item().Text($"Docente: {plan.Docente?.Nombres} {plan.Docente?.Apellidos}");
                    col.Item().Text($"Curso: {plan.Curso?.Nombre} | Año Académico: {plan.AnoAcademico}");

                    RichTextExportHelper.AddPdfSection(col, "Descripción General", plan.Descripcion);
                    RichTextExportHelper.AddPdfSection(col, "Situación de Aprendizaje", plan.SituacionAprendizaje);
                    RichTextExportHelper.AddPdfSection(col, "Competencias Fundamentales", plan.CompetenciasFundamentales);
                    RichTextExportHelper.AddPdfSection(col, "Competencias Específicas", plan.CompetenciasEspecificas);
                    RichTextExportHelper.AddPdfSection(col, "Contenidos Conceptuales", plan.ContenidosConceptuales);
                    RichTextExportHelper.AddPdfSection(col, "Contenidos Procedimentales", plan.ContenidosProcedimentales);
                    RichTextExportHelper.AddPdfSection(col, "Contenidos Actitudinales", plan.ContenidosActitudinales);
                    RichTextExportHelper.AddPdfSection(col, "Indicadores de Logro", plan.IndicadoresLogro);
                    RichTextExportHelper.AddPdfSection(col, "Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
                    RichTextExportHelper.AddPdfSection(col, "Recursos Didácticos", string.IsNullOrWhiteSpace(plan.RecursosDidacticos) ? plan.Recursos : plan.RecursosDidacticos);
                    RichTextExportHelper.AddPdfSection(col, "Ejes Transversales", plan.EjesTransversales);
                    RichTextExportHelper.AddPdfSection(col, "Actividades de Evaluación", plan.ActividadesEvaluacion);
                    RichTextExportHelper.AddPdfSection(col, "Observaciones", plan.Observaciones);
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

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    col.Item().Text($"Unidad: {plan.TituloUnidad}").Bold().FontSize(14);

                    RichTextExportHelper.AddPdfSection(col, "Situación de Aprendizaje", plan.SituacionAprendizaje);
                    RichTextExportHelper.AddPdfSection(col, "Competencias Fundamentales", plan.CompetenciasFundamentales);
                    RichTextExportHelper.AddPdfSection(col, "Competencias Específicas", plan.CompetenciasEspecificas);
                    RichTextExportHelper.AddPdfSection(col, "Contenidos Conceptuales", plan.ContenidosConceptuales);
                    RichTextExportHelper.AddPdfSection(col, "Contenidos Procedimentales", plan.ContenidosProcedimentales);
                    RichTextExportHelper.AddPdfSection(col, "Contenidos Actitudinales", plan.ContenidosActitudinales);
                    RichTextExportHelper.AddPdfSection(col, "Indicadores de Logro", plan.IndicadoresLogro);
                    RichTextExportHelper.AddPdfSection(col, "Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
                    RichTextExportHelper.AddPdfSection(col, "Recursos Didácticos", plan.RecursosDidacticos);
                    RichTextExportHelper.AddPdfSection(col, "Actividades de Evaluación", plan.ActividadesEvaluacion);
                    RichTextExportHelper.AddPdfSection(col, "Ejes Transversales", plan.EjesTransversales);
                    RichTextExportHelper.AddPdfSection(col, "Observaciones", plan.Observaciones);
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

                page.Header().Text($"Planificación Diaria - {plan.Fecha:dd/MM/yyyy}").SemiBold().FontSize(18).FontColor(Colors.Blue.Darken2);

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                {
                    RichTextExportHelper.AddPdfSection(col, "Intención Pedagógica", plan.IntencionPedagogica);
                    RichTextExportHelper.AddPdfSection(col, $"Inicio ({plan.TiempoInicioMinutos} min)", plan.ActividadesInicio);
                    RichTextExportHelper.AddPdfSection(col, $"Desarrollo ({plan.TiempoDesarrolloMinutos} min)", plan.ActividadesDesarrollo);
                    RichTextExportHelper.AddPdfSection(col, $"Cierre ({plan.TiempoCierreMinutos} min)", plan.ActividadesCierre);
                    RichTextExportHelper.AddPdfSection(col, "Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
                    RichTextExportHelper.AddPdfSection(col, "Organización de Estudiantes", plan.OrganizacionEstudiantes);
                    RichTextExportHelper.AddPdfSection(col, "Recursos", plan.Recursos);
                    RichTextExportHelper.AddPdfSection(col, "Vocabulario del Día", plan.VocabularioDia);
                    RichTextExportHelper.AddPdfSection(col, "Lecturas Recomendadas", plan.LecturasRecomendadas);
                    RichTextExportHelper.AddPdfSection(col, "Observaciones", plan.Observaciones);
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerarWordPlanAnual(Planificacion plan)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new OpenXmlDocument(new Body());
            var body = mainPart.Document.Body!;

            body.Append(RichTextExportHelper.CreateWordParagraph("Planificación Anual MINERD", true, "32"));
            body.Append(RichTextExportHelper.CreateWordParagraph($"Título: {plan.Titulo}"));
            body.Append(RichTextExportHelper.CreateWordParagraph($"Docente: {plan.Docente?.Nombres} {plan.Docente?.Apellidos}"));
            body.Append(RichTextExportHelper.CreateWordParagraph($"Curso: {plan.Curso?.Nombre} | Año Académico: {plan.AnoAcademico}"));
            body.Append(RichTextExportHelper.CreateWordParagraph(string.Empty));

            RichTextExportHelper.AddWordSection(doc, body, "Descripción General", plan.Descripcion);
            RichTextExportHelper.AddWordSection(doc, body, "Situación de Aprendizaje", plan.SituacionAprendizaje);
            RichTextExportHelper.AddWordSection(doc, body, "Competencias Fundamentales", plan.CompetenciasFundamentales);
            RichTextExportHelper.AddWordSection(doc, body, "Competencias Específicas", plan.CompetenciasEspecificas);
            RichTextExportHelper.AddWordSection(doc, body, "Contenidos Conceptuales", plan.ContenidosConceptuales);
            RichTextExportHelper.AddWordSection(doc, body, "Contenidos Procedimentales", plan.ContenidosProcedimentales);
            RichTextExportHelper.AddWordSection(doc, body, "Contenidos Actitudinales", plan.ContenidosActitudinales);
            RichTextExportHelper.AddWordSection(doc, body, "Indicadores de Logro", plan.IndicadoresLogro);
            RichTextExportHelper.AddWordSection(doc, body, "Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
            RichTextExportHelper.AddWordSection(doc, body, "Recursos Didácticos", string.IsNullOrWhiteSpace(plan.RecursosDidacticos) ? plan.Recursos : plan.RecursosDidacticos);
            RichTextExportHelper.AddWordSection(doc, body, "Ejes Transversales", plan.EjesTransversales);
            RichTextExportHelper.AddWordSection(doc, body, "Actividades de Evaluación", plan.ActividadesEvaluacion);
            RichTextExportHelper.AddWordSection(doc, body, "Observaciones", plan.Observaciones);

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    public byte[] GenerarWordPlanMensual(PlanificacionMensual plan)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new OpenXmlDocument(new Body());
            var body = mainPart.Document.Body!;

            body.Append(RichTextExportHelper.CreateWordParagraph($"Planificación Mensual - {plan.Mes}", true, "32"));
            body.Append(RichTextExportHelper.CreateWordParagraph($"Unidad: {plan.TituloUnidad}"));
            body.Append(RichTextExportHelper.CreateWordParagraph(string.Empty));

            RichTextExportHelper.AddWordSection(doc, body, "Situación de Aprendizaje", plan.SituacionAprendizaje);
            RichTextExportHelper.AddWordSection(doc, body, "Competencias Fundamentales", plan.CompetenciasFundamentales);
            RichTextExportHelper.AddWordSection(doc, body, "Competencias Específicas", plan.CompetenciasEspecificas);
            RichTextExportHelper.AddWordSection(doc, body, "Contenidos Conceptuales", plan.ContenidosConceptuales);
            RichTextExportHelper.AddWordSection(doc, body, "Contenidos Procedimentales", plan.ContenidosProcedimentales);
            RichTextExportHelper.AddWordSection(doc, body, "Contenidos Actitudinales", plan.ContenidosActitudinales);
            RichTextExportHelper.AddWordSection(doc, body, "Indicadores de Logro", plan.IndicadoresLogro);
            RichTextExportHelper.AddWordSection(doc, body, "Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
            RichTextExportHelper.AddWordSection(doc, body, "Recursos Didácticos", plan.RecursosDidacticos);
            RichTextExportHelper.AddWordSection(doc, body, "Actividades de Evaluación", plan.ActividadesEvaluacion);
            RichTextExportHelper.AddWordSection(doc, body, "Ejes Transversales", plan.EjesTransversales);
            RichTextExportHelper.AddWordSection(doc, body, "Observaciones", plan.Observaciones);

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }

    public byte[] GenerarWordPlanDiario(PlanificacionDiaria plan)
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = doc.AddMainDocumentPart();
            mainPart.Document = new OpenXmlDocument(new Body());
            var body = mainPart.Document.Body!;

            body.Append(RichTextExportHelper.CreateWordParagraph($"Planificación Diaria - {plan.Fecha:dd/MM/yyyy}", true, "32"));
            body.Append(RichTextExportHelper.CreateWordParagraph(string.Empty));

            RichTextExportHelper.AddWordSection(doc, body, "Intención Pedagógica", plan.IntencionPedagogica);
            RichTextExportHelper.AddWordSection(doc, body, $"Inicio ({plan.TiempoInicioMinutos} min)", plan.ActividadesInicio);
            RichTextExportHelper.AddWordSection(doc, body, $"Desarrollo ({plan.TiempoDesarrolloMinutos} min)", plan.ActividadesDesarrollo);
            RichTextExportHelper.AddWordSection(doc, body, $"Cierre ({plan.TiempoCierreMinutos} min)", plan.ActividadesCierre);
            RichTextExportHelper.AddWordSection(doc, body, "Estrategias de Enseñanza", plan.EstrategiasEnsenanza);
            RichTextExportHelper.AddWordSection(doc, body, "Organización de Estudiantes", plan.OrganizacionEstudiantes);
            RichTextExportHelper.AddWordSection(doc, body, "Recursos", plan.Recursos);
            RichTextExportHelper.AddWordSection(doc, body, "Vocabulario del Día", plan.VocabularioDia);
            RichTextExportHelper.AddWordSection(doc, body, "Lecturas Recomendadas", plan.LecturasRecomendadas);
            RichTextExportHelper.AddWordSection(doc, body, "Observaciones", plan.Observaciones);

            mainPart.Document.Save();
        }

        return stream.ToArray();
    }
}
