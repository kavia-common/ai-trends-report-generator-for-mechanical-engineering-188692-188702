using System;

namespace Backend.Models
{
    /// <summary>
    /// Represents an AI trend in mechanical engineering.
    /// </summary>
    public class Trend
    {
        /// <summary>
        /// Unique identifier of the trend.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Short title of the trend.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Summary description of the trend.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Source URL where the trend originated.
        /// </summary>
        public string SourceUrl { get; set; } = string.Empty;

        /// <summary>
        /// Publication or discovery date of the trend.
        /// </summary>
        public DateTime Date { get; set; }
    }
}
