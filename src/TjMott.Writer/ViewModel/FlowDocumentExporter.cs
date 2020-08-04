using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Docx = Xceed.Words.NET;

namespace TjMott.Writer.ViewModel
{
    public static class FlowDocumentExporter
    {
        public static void ExportItem(IExportToWordDocument item, string outputPath, string templatePath)
        {
            using (Docx.DocX doc = Docx.DocX.Create(outputPath))
            {
                if (templatePath != null)
                {
                    doc.ApplyTemplate(templatePath);
                }
                item.ExportToWord(doc);
                doc.Save();
            }
        }

        public static void AddBlock(Block block, Docx.DocX doc)
        {
            if (block is Paragraph)
            {
                Paragraph p = (Paragraph)block;
                FlowDocumentExporter.AddParagraph(p, doc);
            }
            else if (block is List)
            {
                List list = (List)block;
                AddList(list, doc);
            }
        }


        public static void AddParagraph(Paragraph paragraph, Docx.DocX doc)
        {
            Xceed.Document.NET.Paragraph p = doc.InsertParagraph();
            foreach (var inline in paragraph.Inlines)
            {
                AddInline(inline, p);
            }
        }
        public static void AddList(List list, Docx.DocX doc)
        {
            // TODO: Figure this out. ListItems require a paragraph
            // to add, but the paragraph constructor is private. ??
            /*
            Docx.ListItemType listType;
            if (list.MarkerStyle == TextMarkerStyle.Disc)
            {
                listType = Docx.ListItemType.Bulleted;
            }
            else
            {
                listType = Docx.ListItemType.Numbered;
            }
            
            Docx.List dList = doc.AddList(listType: listType);
            foreach (var item in list.ListItems)
            {
                foreach (var block in item.Blocks)
                {
                    AddBlock(block, doc);
                }
            }*/
        }
        public static void AddInline(Inline inline, Xceed.Document.NET.Paragraph p, Xceed.Document.NET.Formatting inheritFormat = null)
        {
            if (inline is Run)
            {
                Run run = (Run)inline;
                Xceed.Document.NET.Formatting f = new Xceed.Document.NET.Formatting();
                Color c = (run.Foreground as SolidColorBrush).Color;
                f.FontColor = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
                f.FontFamily = new Xceed.Document.NET.Font(run.FontFamily.Source);
                if (run.FontWeight == FontWeights.Bold)
                {
                    f.Bold = true;
                }
                if (run.FontStyle == FontStyles.Italic)
                {
                    f.Italic = true;
                }
                foreach (var dec in run.TextDecorations)
                {
                    if (dec.Location == TextDecorationLocation.Underline)
                    {
                        f.UnderlineStyle = Xceed.Document.NET.UnderlineStyle.singleLine;
                        f.UnderlineColor = f.FontColor;
                    }
                }

                f.Size = run.FontSize;

                if (inheritFormat != null)
                    f = inheritFormat;

                p.Append(run.Text, f);
            }
            else if (inline is Span)
            {
                Span span = (Span)inline;

                Xceed.Document.NET.Formatting f = new Xceed.Document.NET.Formatting();
                Color c = (span.Foreground as SolidColorBrush).Color;
                f.FontColor = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
                f.FontFamily = new Xceed.Document.NET.Font(span.FontFamily.Source);
                if (span.FontWeight == FontWeights.Bold)
                {
                    f.Bold = true;
                }
                if (span.FontStyle == FontStyles.Italic)
                {
                    f.Italic = true;
                }
                foreach (var dec in span.TextDecorations)
                {
                    if (dec.Location == TextDecorationLocation.Underline)
                    {
                        f.UnderlineStyle = Xceed.Document.NET.UnderlineStyle.singleLine;
                        f.UnderlineColor = f.FontColor;
                    }
                }

                f.Size = span.FontSize;

                if (inheritFormat != null)
                    f = inheritFormat;

                foreach (var spanInline in span.Inlines)
                {
                    AddInline(spanInline, p, f);
                }
            }
        }
    }
}
