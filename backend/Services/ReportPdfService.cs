using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Backend.Models;

namespace Backend.Services
{
    /// <summary>
    /// Service responsible for rendering report data to a PDF document.
    /// This implementation is self-contained and does not rely on external libraries.
    /// It generates a minimal PDF with basic text layout.
    /// </summary>
    public interface IReportPdfService
    {
        // PUBLIC_INTERFACE
        byte[] GeneratePdf(IEnumerable<Trend> trends);
    }

    /// <inheritdoc />
    public class ReportPdfService : IReportPdfService
    {
        /// <summary>
        /// Generates a simple PDF with headings and trend content using only basic PDF primitives.
        /// </summary>
        /// <param name="trends">The trends to include.</param>
        /// <returns>PDF bytes.</returns>
        // PUBLIC_INTERFACE
        public byte[] GeneratePdf(IEnumerable<Trend> trends)
        {
            // Compose lines of text for the PDF content
            var lines = new List<string>
            {
                "AI Trends Report - Mechanical Engineering",
                $"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC",
                "",
                "Latest Trends"
            };

            foreach (var t in trends)
            {
                lines.Add($"• {t.Title}");
                if (!string.IsNullOrWhiteSpace(t.Summary))
                    lines.Add($"   Summary: {t.Summary}");
                if (!string.IsNullOrWhiteSpace(t.SourceUrl))
                    lines.Add($"   Source: {t.SourceUrl}");
                lines.Add($"   Date: {t.Date:yyyy-MM-dd}");
                lines.Add("");
            }

            var builder = new SimplePdfBuilder();
            return builder.BuildSinglePage(lines);
        }
    }

    /// <summary>
    /// Minimal PDF builder that produces a single A4 page with text lines using the built-in Helvetica font.
    /// Intended for simple reports without images or advanced layout.
    /// </summary>
    internal sealed class SimplePdfBuilder
    {
        // A4 size in points: 595 x 842
        private const int PageWidth = 595;
        private const int PageHeight = 842;

        public byte[] BuildSinglePage(IEnumerable<string> lines)
        {
            using var ms = new MemoryStream();
            var offsets = new List<long>();

            void WriteLine(string s)
            {
                var bytes = Encoding.ASCII.GetBytes(s);
                ms.Write(bytes, 0, bytes.Length);
            }

            // PDF header
            WriteLine("%PDF-1.4\n");

            // Object 1: Catalog
            offsets.Add(ms.Position);
            WriteLine("1 0 obj\n");
            WriteLine("<< /Type /Catalog /Pages 2 0 R >>\n");
            WriteLine("endobj\n");

            // Object 2: Pages
            offsets.Add(ms.Position);
            WriteLine("2 0 obj\n");
            WriteLine("<< /Type /Pages /Kids [3 0 R] /Count 1 >>\n");
            WriteLine("endobj\n");

            // Object 3: Page
            offsets.Add(ms.Position);
            WriteLine("3 0 obj\n");
            WriteLine("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] ");
            WriteLine("/Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>\n");
            WriteLine("endobj\n");

            // Object 4: Font (Helvetica)
            offsets.Add(ms.Position);
            WriteLine("4 0 obj\n");
            WriteLine("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>\n");
            WriteLine("endobj\n");

            // Build stream content
            var contentStream = BuildContentStream(lines);

            // Object 5: Contents stream
            offsets.Add(ms.Position);
            WriteLine("5 0 obj\n");
            WriteLine($"<< /Length {contentStream.Length} >>\n");
            WriteLine("stream\n");
            ms.Write(contentStream, 0, contentStream.Length);
            WriteLine("\nendstream\n");
            WriteLine("endobj\n");

            // XRef
            var xrefPosition = ms.Position;
            WriteLine("xref\n");
            WriteLine("0 6\n");
            // object 0 (free)
            WriteLine("0000000000 65535 f \n");
            for (int i = 0; i < offsets.Count; i++)
            {
                WriteLine($"{offsets[i]:D10} 00000 n \n");
            }

            // Trailer
            WriteLine("trailer\n");
            WriteLine("<< /Size 6 /Root 1 0 R >>\n");
            WriteLine("startxref\n");
            WriteLine($"{xrefPosition}\n");
            WriteLine("%%EOF");

            return ms.ToArray();
        }

        private static byte[] BuildContentStream(IEnumerable<string> lines)
        {
            // Start near top-left with some margins
            const int leftMargin = 72; // 1 inch
            const int topY = 750;
            const int fontSizeTitle = 16;
            const int fontSizeNormal = 11;
            const int leading = 16;

            var sb = new StringBuilder();
            sb.Append("BT\n");
            sb.Append("/F1 ").Append(fontSizeTitle).Append(" Tf\n");
            sb.Append("1 0 0 1 ").Append(leftMargin).Append(" ").Append(topY).Append(" Tm\n");
            sb.Append(leading).Append(" TL\n");

            bool first = true;
            foreach (var line in lines)
            {
                int currentFontSize = first ? fontSizeTitle : fontSizeNormal;
                if (!first)
                {
                    // move to next line
                    sb.Append("T*\n");
                }
                sb.Append("/F1 ").Append(currentFontSize).Append(" Tf\n");
                sb.Append("(").Append(EscapePdfText(line)).Append(") Tj\n");
                first = false;
            }

            sb.Append("\nET");

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        private static string EscapePdfText(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input
                .Replace("\\", "\\\\")
                .Replace("(", "\\(")
                .Replace(")", "\\)")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }
    }
}
