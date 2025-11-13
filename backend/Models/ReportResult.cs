using System;

namespace Backend.Models
{
    /// <summary>
    /// Result returned after a report is generated.
    /// </summary>
    public class ReportResult
    {
        /// <summary>
        /// The unique identifier for the generated report.
        /// </summary>
        public Guid ReportId { get; set; }
    }
}
