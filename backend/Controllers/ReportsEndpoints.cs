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

            return app;
        }
    }
}
