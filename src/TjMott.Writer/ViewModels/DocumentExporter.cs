using Avalonia.Media;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Document.NET;

namespace TjMott.Writer.ViewModels
{
    public static class DocumentExporter
    {
        public static void ApplyFormatting(Formatting format, Paragraph paragraph, JObject attributes)
        {
            if (attributes.ContainsKey("italic") )
            {
                format.Italic = attributes["italic"].Value<bool>();
            }
            if (attributes.ContainsKey("bold"))
            {
                format.Bold = attributes["bold"].Value<bool>();
            }
            if (attributes.ContainsKey("underline"))
            {
                if (attributes["underline"].Value<bool>())
                    format.UnderlineStyle = UnderlineStyle.singleLine;
                else
                    format.UnderlineStyle = UnderlineStyle.none;
            }
            if (attributes.ContainsKey("strike"))
            {
                if (attributes["strike"].Value<bool>())
                    format.StrikeThrough = StrikeThrough.strike;
                else
                    format.StrikeThrough = StrikeThrough.none;
            }
            if (attributes.ContainsKey("font"))
            {
                format.FontFamily = new Font(attributes["font"].Value<string>());
            }
            if (attributes.ContainsKey("size"))
            {
                string sizeString = attributes["size"].Value<string>();
                if (sizeString.EndsWith("px"))
                {
                    sizeString = sizeString.Substring(0, sizeString.Length - 2);
                    format.Size = double.Parse(sizeString);
                }
            }
            if (attributes.ContainsKey("color"))
            {
                string colorString = attributes["color"].Value<string>();
                Color c = Color.Parse(colorString);
                format.FontColor = System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
            }

            if (attributes.ContainsKey("script"))
            {
                string script = attributes["script"].Value<string>();
                if (script == "sub")
                    format.Script = Script.subscript;
                else if (script == "super")
                    format.Script = Script.superscript;
            }
            if (attributes.ContainsKey("list"))
            {
                string list = attributes["list"].Value<string>();
                // Not really sure how to do this.
            }

            // Paragraph stuff.
            if (paragraph != null)
            {
                if (attributes.ContainsKey("align"))
                {
                    string align = attributes["align"].Value<string>();
                    if (align == "left")
                        paragraph.Alignment = Alignment.left;
                    else if (align == "right")
                        paragraph.Alignment = Alignment.right;
                    else if (align == "justify")
                        paragraph.Alignment = Alignment.both;
                    else if (align == "center")
                        paragraph.Alignment = Alignment.center;
                }
                if (attributes.ContainsKey("indent"))
                {
                    int indentLevel = attributes["indent"].Value<int>();
                    // IndentLevel is read-only...?
                    // paragraph.IndentLevel = indentLevel;
                }
            }
        }
    }
}
