using System;
using System.Collections.Generic;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Backend.Controllers
{
    /// <summary>
    /// Minimal API endpoints for generating and downloading reports.
    /// </summary>
    public static class ReportsEndpoints
    {
        /// <summary>
        /// Maps the report generation and download endpoints under /api/reports.
        /// </summary>
        // PUBLIC_INTERFACE
        public static IEndpointRouteBuilder MapReportsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/reports").WithTags("Reports");

            // POST /api/reports
            group.MapPost("/", (ITrendsService trendsService, IReportService reportService) =>
            {
                IEnumerable<Trend> trends = trendsService.GetLatestTrends();
                var id = reportService.GenerateReport(trends);
                return Results.Ok(new ReportResult { ReportId = id });
            })
            .WithSummary("Generate a .docx report")
            .WithDescription("Generates a .docx report from current trends and returns { reportId }.")
            .Produces<ReportResult>(StatusCodes.Status200OK);

            // GET /api/reports/{id}/download
            group.MapGet("/{id:guid}/download", (Guid id, IReportService reportService) =>
            {
                if (reportService.TryGetReport(id, out var content) && content != null)
                {
                    const string fileName = "AI-Trends-Report.docx";
                    const string contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    return Results.File(content, contentType, fileName);
                }

                return Results.NotFound(new { message = "Report not found" });
            })
            .WithSummary("Download a generated report")
            .WithDescription("Streams the stored .docx report for the specified report id.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // GET /api/reports/{id}/pdf
            group.MapGet("/{id:guid}/pdf", (Guid id, bool? inline, IReportService reportService, IReportPdfService pdfService, HttpContext httpContext) =>
            {
                if (!reportService.TryGetReportData(id, out var trends) || trends == null)
                {
                    return Results.NotFound(new { message = "Report not found" });
                }

                try
                {
                    var pdfBytes = pdfService.GeneratePdf(trends);
                    const string fileName = "AI-Trends-Report.pdf";
                    const string contentType = "application/pdf";

                    // Set Content-Disposition based on inline flag
                    var disposition = (inline ?? false) ? "inline" : "attachment";
                    httpContext.Response.Headers["Content-Disposition"] = $"{disposition}; filename=\"{fileName}\"";

                    // Return file without filename in Results.File to avoid forcing attachment by framework
                    return Results.File(pdfBytes, contentType);
                }
                catch (Exception ex)
                {
                    return Results.Problem(
                        title: "PDF render failed",
                        detail: ex.Message,
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithSummary("Get a PDF version of the report")
            .WithDescription("Generates and streams a PDF built from the same data as the .docx report. Use ?inline=true to view in-browser.")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError);

            return app;
        }
    }
}
