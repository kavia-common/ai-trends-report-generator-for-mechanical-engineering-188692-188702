using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Backend.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Backend.Services
{
    /// <summary>
    /// Service that generates .docx reports and stores them in-memory.
    /// Also retains the raw report data for subsequent PDF generation.
    /// </summary>
    public interface IReportService
    {
        // PUBLIC_INTERFACE
        Guid GenerateReport(IEnumerable<Trend> trends);

        // PUBLIC_INTERFACE
        bool TryGetReport(Guid id, out byte[]? content);

        /// <summary>
        /// Attempts to retrieve the raw trends used to generate a report.
        /// </summary>
        /// <param name="id">Report id.</param>
        /// <param name="trends">Trends collection if found.</param>
        /// <returns>true if found; otherwise false.</returns>
        // PUBLIC_INTERFACE
        bool TryGetReportData(Guid id, out IEnumerable<Trend>? trends);
    }

    /// <inheritdoc />
    public class ReportService : IReportService
    {
        private static readonly ConcurrentDictionary<Guid, byte[]> Reports = new();
        private static readonly ConcurrentDictionary<Guid, List<Trend>> ReportTrends = new();

        /// <summary>
        /// Generates a .docx report from a list of trends and stores the bytes in-memory.
        /// Also stores the underlying data for later PDF generation.
        /// </summary>
        /// <param name="trends">The trends to include in the report.</param>
        /// <returns>Guid of the generated report.</returns>
        // PUBLIC_INTERFACE
        public Guid GenerateReport(IEnumerable<Trend> trends)
        {
            var trendList = new List<Trend>(trends ?? new List<Trend>());
            byte[] bytes = BuildDocx(trendList);
            var id = Guid.NewGuid();
            Reports[id] = bytes;
            ReportTrends[id] = trendList;
            return id;
        }

        /// <summary>
        /// Attempts to retrieve a previously generated report by id.
        /// </summary>
        /// <param name="id">Report id.</param>
        /// <param name="content">byte[] content if found.</param>
        /// <returns>true if found; otherwise false.</returns>
        // PUBLIC_INTERFACE
        public bool TryGetReport(Guid id, out byte[]? content)
        {
            if (Reports.TryGetValue(id, out var found))
            {
                content = found;
                return true;
            }

            content = null;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve the underlying trends used for a report.
        /// </summary>
        /// <param name="id">Report id.</param>
        /// <param name="trends">Trends if found.</param>
        /// <returns>true if found; otherwise false.</returns>
        // PUBLIC_INTERFACE
        public bool TryGetReportData(Guid id, out IEnumerable<Trend>? trends)
        {
            if (ReportTrends.TryGetValue(id, out var list))
            {
                trends = list;
                return true;
            }

            trends = null;
            return false;
        }

        /// <summary>
        /// Builds a minimal .docx document with a heading and bullet list of trends.
        /// </summary>
        private static byte[] BuildDocx(IEnumerable<Trend> trends)
        {
            using var ms = new MemoryStream();
            using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document, true))
            {
                var mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                var body = new Body();

                // Title
                body.AppendChild(CreateParagraph("AI Trends Report - Mechanical Engineering", bold: true, fontSize: "32"));

                // Date
                body.AppendChild(CreateParagraph($"Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC", italic: true, fontSize: "20"));

                // Spacer
                body.AppendChild(CreateParagraph(""));

                // Section heading
                body.AppendChild(CreateParagraph("Latest Trends", bold: true, fontSize: "28"));

                // Bullet list of trends
                foreach (var trend in trends)
                {
                    // Title bullet
                    body.AppendChild(CreateBulletParagraph(trend.Title));
                    // Summary sub-bullet
                    body.AppendChild(CreateBulletParagraph($"Summary: {trend.Summary}", level: 1));
                    // Source sub-bullet
                    body.AppendChild(CreateBulletParagraph($"Source: {trend.SourceUrl}", level: 1));
                    // Date sub-bullet
                    body.AppendChild(CreateBulletParagraph($"Date: {trend.Date:yyyy-MM-dd}", level: 1));
                    // Spacer
                    body.AppendChild(CreateParagraph(""));
                }

                mainPart.Document.Append(body);
                mainPart.Document.Save();

                // Add minimal numbering for bullets
                EnsureNumberingDefinitions(doc);
            }

            return ms.ToArray();
        }

        private static Paragraph CreateParagraph(string text, bool bold = false, bool italic = false, string? fontSize = null)
        {
            var runProps = new RunProperties();
            if (bold) runProps.Append(new Bold());
            if (italic) runProps.Append(new Italic());
            if (!string.IsNullOrWhiteSpace(fontSize))
            {
                // size is in half-points
                runProps.Append(new FontSize { Val = fontSize });
            }

            var run = new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            var para = new Paragraph(new ParagraphProperties(), run);
            return para;
        }

        private static Paragraph CreateBulletParagraph(string text, int level = 0)
        {
            var numberingProps = new NumberingProperties(
                new NumberingLevelReference { Val = level },
                new NumberingId { Val = 1 });

            var pPr = new ParagraphProperties(numberingProps);
            var run = new Run(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
            var para = new Paragraph(pPr, run);
            return para;
        }

        /// <summary>
        /// Ensures the numbering definition exists for bullet lists with multiple levels.
        /// </summary>
        private static void EnsureNumberingDefinitions(WordprocessingDocument doc)
        {
            var numberingPart = doc.MainDocumentPart!.NumberingDefinitionsPart;
            if (numberingPart == null)
            {
                numberingPart = doc.MainDocumentPart.AddNewPart<NumberingDefinitionsPart>();
                numberingPart.Numbering = new Numbering();
            }

            // Abstract numbering definition (bullets)
            var abstractNum = new AbstractNum(new Level(
                    new NumberingFormat { Val = NumberFormatValues.Bullet },
                    new LevelText { Val = "•" },
                    new LevelJustification { Val = LevelJustificationValues.Left }
                )
                { LevelIndex = 0 });

            // Add three levels
            for (int i = 1; i <= 8; i++)
            {
                abstractNum.AppendChild(new Level(
                        new NumberingFormat { Val = NumberFormatValues.Bullet },
                        new LevelText { Val = "•" },
                        new LevelJustification { Val = LevelJustificationValues.Left }
                    )
                    { LevelIndex = i });
            }

            abstractNum.AbstractNumberId = 1;

            // If an abstract numbering with same id exists, skip adding duplicate
            var existingAbstract = numberingPart.Numbering!.Elements<AbstractNum>();
            bool hasAbstract1 = false;
            foreach (var a in existingAbstract)
            {
                if (a.AbstractNumberId == 1)
                {
                    hasAbstract1 = true;
                    break;
                }
            }

            if (!hasAbstract1)
            {
                numberingPart.Numbering.AppendChild(abstractNum);
            }

            // Numbering instance referencing abstract id 1
            var existingNums = numberingPart.Numbering!.Elements<NumberingInstance>();
            bool hasNum1 = false;
            foreach (var n in existingNums)
            {
                if (n.NumberID == 1)
                {
                    hasNum1 = true;
                    break;
                }
            }

            if (!hasNum1)
            {
                numberingPart.Numbering.AppendChild(new NumberingInstance(
                    new AbstractNumId { Val = 1 }
                )
                { NumberID = 1 });
            }

            numberingPart.Numbering.Save();
        }
    }
}
