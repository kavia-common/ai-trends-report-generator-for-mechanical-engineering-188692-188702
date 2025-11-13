using System;
using System.Collections.Generic;
using Backend.Models;

namespace Backend.Services
{
    /// <summary>
    /// Service providing the latest AI trends for mechanical engineering.
    /// </summary>
    public interface ITrendsService
    {
        // PUBLIC_INTERFACE
        IEnumerable<Trend> GetLatestTrends();
    }

    /// <inheritdoc />
    public class TrendsService : ITrendsService
    {
        /// <summary>
        /// Returns a curated static list of current AI trends in mechanical engineering.
        /// </summary>
        // PUBLIC_INTERFACE
        public IEnumerable<Trend> GetLatestTrends()
        {
            var now = DateTime.UtcNow.Date;
            return new List<Trend>
            {
                new Trend
                {
                    Id = Guid.NewGuid(),
                    Title = "Physics-informed Neural Networks (PINNs) for Simulation",
                    Summary = "Using PINNs to accelerate FEA/CFD simulations by embedding governing equations into model training.",
                    SourceUrl = "https://example.com/pinns-mech",
                    Date = now.AddDays(-2)
                },
                new Trend
                {
                    Id = Guid.NewGuid(),
                    Title = "Generative Design with AI",
                    Summary = "Optimization of mechanical parts using AI-driven topological generation balancing weight, stress, and manufacturability.",
                    SourceUrl = "https://example.com/generative-design",
                    Date = now.AddDays(-5)
                },
                new Trend
                {
                    Id = Guid.NewGuid(),
                    Title = "AI-enabled Predictive Maintenance",
                    Summary = "Vibration and acoustic sensing models to predict failure in rotating machinery and bearings.",
                    SourceUrl = "https://example.com/predictive-maintenance",
                    Date = now.AddDays(-7)
                },
                new Trend
                {
                    Id = Guid.NewGuid(),
                    Title = "Digital Twins with ML Surrogates",
                    Summary = "Hybrid approach combining physics-based digital twins with ML surrogates for rapid scenario testing.",
                    SourceUrl = "https://example.com/digital-twin-ml",
                    Date = now.AddDays(-10)
                },
                new Trend
                {
                    Id = Guid.NewGuid(),
                    Title = "Quality Inspection via Vision Transformers",
                    Summary = "Automated defect detection on production lines using ViT-based anomaly segmentation.",
                    SourceUrl = "https://example.com/vision-transformers-qc",
                    Date = now.AddDays(-12)
                }
            };
        }
    }
}
