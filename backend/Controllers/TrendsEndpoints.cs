using System.Collections.Generic;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Backend.Controllers
{
    /// <summary>
    /// Minimal API endpoints for trends.
    /// </summary>
    public static class TrendsEndpoints
    {
        /// <summary>
        /// Maps the trends endpoints under /api/trends.
        /// </summary>
        // PUBLIC_INTERFACE
        public static IEndpointRouteBuilder MapTrendsEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/trends").WithTags("Trends");

            group.MapGet("/", (ITrendsService trendsService) =>
            {
                IEnumerable<Trend> trends = trendsService.GetLatestTrends();
                return Results.Ok(trends);
            })
            .WithSummary("Get latest AI trends")
            .WithDescription("Returns a list of mocked current AI trends in mechanical engineering.")
            .Produces<IEnumerable<Trend>>(StatusCodes.Status200OK);

            return app;
        }
    }
}
