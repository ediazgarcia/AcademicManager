using System.Net;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlAgilityPack;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AcademicManager.Application.Services;

public static class RichTextExportHelper
{
    private static readonly HashSet<string> ParagraphTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "p", "div", "h1", "h2", "h3", "h4", "h5", "h6", "blockquote", "pre"
    };

    private readonly record struct TextStyle(bool Bold, bool Italic, bool Underline);

    private sealed record StyledRun(string Text, bool Bold, bool Italic, bool Underline);

    private sealed record RichBlock(IReadOnlyList<StyledRun> Runs, bool IsListItem, string Marker);

    public static void AddPdfSection(ColumnDescriptor column, string title, string? html, float leftPadding = 10)
    {
        column.Item().PaddingTop(6).Text(title).Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
        AddPdfContent(column, html, leftPadding);
    }

    public static void AddPdfContent(ColumnDescriptor column, string? html, float leftPadding = 0)
    {
        var blocks = ParseBlocks(html);
        if (blocks.Count == 0)
        {
            column.Item().PaddingLeft(leftPadding).PaddingBottom(2).Text("N/A");
            return;
        }

        foreach (var block in blocks)
        {
            if (block.IsListItem)
            {
                column.Item().PaddingLeft(leftPadding).PaddingBottom(2).Row(row =>
                {
                    row.ConstantItem(18).Text(block.Marker);
                    row.RelativeItem().Text(text => AppendPdfRuns(text, block.Runs));
                });
            }
            else
            {
                column.Item().PaddingLeft(leftPadding).PaddingBottom(2).Text(text => AppendPdfRuns(text, block.Runs));
            }
        }
    }

    public static void AddWordSection(WordprocessingDocument document, Body body, string title, string? html, string headingSize = "24")
    {
        body.Append(CreateWordParagraph(title, true, headingSize));
        AddWordContent(document, body, html);
        body.Append(CreateWordParagraph(string.Empty));
    }

    public static void AddWordContent(WordprocessingDocument document, Body body, string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            body.Append(CreateWordParagraph("N/A"));
            return;
        }

        if (!LooksLikeHtml(html))
        {
            AppendPlainWordParagraphs(body, WebUtility.HtmlDecode(html));
            return;
        }

        var mainPart = document.MainDocumentPart ?? throw new InvalidOperationException("MainDocumentPart is required.");
        var altChunkId = $"AltChunk{Guid.NewGuid():N}";
        var altPart = mainPart.AddAlternativeFormatImportPart(AlternativeFormatImportPartType.Html, altChunkId);

        using (var stream = altPart.GetStream(FileMode.Create, FileAccess.Write))
        using (var writer = new StreamWriter(stream))
        {
            writer.Write(BuildWordHtmlDocument(html));
        }

        body.Append(new AltChunk { Id = altChunkId });
    }

    public static Paragraph CreateWordParagraph(string text, bool isBold = false, string fontSize = "22")
    {
        var runProperties = new RunProperties(new FontSize { Val = fontSize });
        if (isBold)
        {
            runProperties.Append(new Bold());
        }

        var run = new Run();
        run.Append(runProperties);
        run.Append(new Text(text ?? string.Empty) { Space = SpaceProcessingModeValues.Preserve });

        var paragraph = new Paragraph();
        paragraph.Append(run);
        return paragraph;
    }

    private static string BuildWordHtmlDocument(string htmlFragment)
    {
        var sanitized = Regex.Replace(htmlFragment, "<(script|style)[^>]*>[\\s\\S]*?</\\1>", string.Empty, RegexOptions.IgnoreCase);
        return $@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <style>
        body {{ font-family: Calibri, Arial, sans-serif; font-size: 11pt; line-height: 1.35; }}
        p {{ margin: 0 0 8pt 0; }}
        ul, ol {{ margin: 0 0 8pt 18pt; }}
        li {{ margin: 0 0 4pt 0; }}
    </style>
</head>
<body>
    {sanitized}
</body>
</html>";
    }

    private static void AppendPlainWordParagraphs(Body body, string text)
    {
        var lines = text
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            body.Append(CreateWordParagraph("N/A"));
            return;
        }

        foreach (var line in lines)
        {
            body.Append(CreateWordParagraph(line));
        }
    }

    private static bool LooksLikeHtml(string value)
    {
        return value.IndexOf('<') >= 0 && value.IndexOf('>') > value.IndexOf('<');
    }

    private static List<RichBlock> ParseBlocks(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return new();
        }

        var source = html.Trim();
        if (!LooksLikeHtml(source))
        {
            return PlainTextToBlocks(WebUtility.HtmlDecode(source));
        }

        try
        {
            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true
            };
            doc.LoadHtml($"<root>{source}</root>");

            var root = doc.DocumentNode.SelectSingleNode("//root");
            if (root == null)
            {
                return PlainTextToBlocks(WebUtility.HtmlDecode(source));
            }

            var blocks = new List<RichBlock>();
            foreach (var child in root.ChildNodes)
            {
                ParseBlockNode(child, blocks);
            }

            if (blocks.Count == 0)
            {
                return PlainTextToBlocks(WebUtility.HtmlDecode(root.InnerText));
            }

            return blocks;
        }
        catch
        {
            return PlainTextToBlocks(WebUtility.HtmlDecode(source));
        }
    }

    private static void ParseBlockNode(HtmlNode node, List<RichBlock> blocks)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = NormalizeInlineText(node.InnerText);
            if (!string.IsNullOrWhiteSpace(text))
            {
                blocks.Add(new RichBlock(new List<StyledRun> { new(text, false, false, false) }, false, string.Empty));
            }

            return;
        }

        if (node.NodeType != HtmlNodeType.Element)
        {
            return;
        }

        var tag = node.Name.ToLowerInvariant();
        if (tag is "ul" or "ol")
        {
            var index = 1;
            foreach (var li in node.Elements("li"))
            {
                var runs = new List<StyledRun>();
                ParseInlineChildren(li, default, runs);
                TrimRuns(runs);
                if (runs.Count > 0)
                {
                    var marker = tag == "ol" ? $"{index++}." : "•";
                    blocks.Add(new RichBlock(runs, true, marker));
                }
            }

            return;
        }

        if (tag == "li")
        {
            var runs = new List<StyledRun>();
            ParseInlineChildren(node, default, runs);
            TrimRuns(runs);
            if (runs.Count > 0)
            {
                blocks.Add(new RichBlock(runs, true, "•"));
            }

            return;
        }

        if (ParagraphTags.Contains(tag))
        {
            var runs = new List<StyledRun>();
            ParseInlineChildren(node, default, runs);
            TrimRuns(runs);
            if (runs.Count > 0)
            {
                blocks.Add(new RichBlock(runs, false, string.Empty));
            }

            return;
        }

        foreach (var child in node.ChildNodes)
        {
            ParseBlockNode(child, blocks);
        }
    }

    private static void ParseInlineChildren(HtmlNode node, TextStyle style, List<StyledRun> runs)
    {
        foreach (var child in node.ChildNodes)
        {
            ParseInlineNode(child, style, runs);
        }
    }

    private static void ParseInlineNode(HtmlNode node, TextStyle style, List<StyledRun> runs)
    {
        if (node.NodeType == HtmlNodeType.Text)
        {
            var text = NormalizeInlineText(node.InnerText);
            if (!string.IsNullOrWhiteSpace(text))
            {
                AddRun(runs, text, style);
            }

            return;
        }

        if (node.NodeType != HtmlNodeType.Element)
        {
            return;
        }

        var tag = node.Name.ToLowerInvariant();
        if (tag == "br")
        {
            AddRun(runs, "\n", style);
            return;
        }

        var nextStyle = style with
        {
            Bold = style.Bold || tag is "strong" or "b",
            Italic = style.Italic || tag is "em" or "i",
            Underline = style.Underline || tag == "u"
        };

        ParseInlineChildren(node, nextStyle, runs);

        if (tag is "p" or "div")
        {
            AddRun(runs, "\n", style);
        }
    }

    private static void AddRun(List<StyledRun> runs, string text, TextStyle style)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        if (runs.Count > 0)
        {
            var last = runs[^1];
            if (last.Bold == style.Bold && last.Italic == style.Italic && last.Underline == style.Underline)
            {
                runs[^1] = last with { Text = last.Text + text };
                return;
            }
        }

        runs.Add(new StyledRun(text, style.Bold, style.Italic, style.Underline));
    }

    private static string NormalizeInlineText(string value)
    {
        var decoded = WebUtility.HtmlDecode(value ?? string.Empty).Replace('\u00A0', ' ');
        return Regex.Replace(decoded, @"\s+", " ");
    }

    private static List<RichBlock> PlainTextToBlocks(string text)
    {
        var lines = text
            .Replace("\r", string.Empty)
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            return new();
        }

        var blocks = new List<RichBlock>(lines.Length);
        foreach (var line in lines)
        {
            var marker = GetPlainTextListMarker(line, out var cleanLine);
            var runs = new List<StyledRun> { new(cleanLine, false, false, false) };
            blocks.Add(new RichBlock(runs, marker != null, marker ?? string.Empty));
        }

        return blocks;
    }

    private static string? GetPlainTextListMarker(string line, out string cleanText)
    {
        var orderedMatch = Regex.Match(line, @"^\s*(\d+)[\.\)]\s+(.+)$");
        if (orderedMatch.Success)
        {
            cleanText = orderedMatch.Groups[2].Value;
            return $"{orderedMatch.Groups[1].Value}.";
        }

        var unorderedMatch = Regex.Match(line, @"^\s*[-*•]\s+(.+)$");
        if (unorderedMatch.Success)
        {
            cleanText = unorderedMatch.Groups[1].Value;
            return "•";
        }

        cleanText = line.Trim();
        return null;
    }

    private static void TrimRuns(List<StyledRun> runs)
    {
        for (var i = 0; i < runs.Count; i++)
        {
            var text = runs[i].Text;
            if (i == 0)
            {
                text = text.TrimStart();
            }

            if (i == runs.Count - 1)
            {
                text = text.TrimEnd();
            }

            runs[i] = runs[i] with { Text = text };
        }

        runs.RemoveAll(r => string.IsNullOrWhiteSpace(r.Text));
    }

    private static void AppendPdfRuns(TextDescriptor text, IReadOnlyList<StyledRun> runs)
    {
        if (runs.Count == 0)
        {
            text.Span(" ");
            return;
        }

        foreach (var run in runs)
        {
            if (string.IsNullOrEmpty(run.Text))
            {
                continue;
            }

            var span = text.Span(run.Text);
            if (run.Bold)
            {
                span.Bold();
            }

            if (run.Italic)
            {
                span.Italic();
            }

            if (run.Underline)
            {
                span.Underline();
            }
        }
    }
}
