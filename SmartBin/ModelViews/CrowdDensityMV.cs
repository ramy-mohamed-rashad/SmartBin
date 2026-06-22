using System.ComponentModel.DataAnnotations;

namespace SmartBin.ModelViews
{
    /// <summary>
    /// Payload sent by the external AI service to report a crowd density reading.
    /// </summary>
    public class CrowdDensityReportMV
    {
        [Required]
        public string DensityLevel { get; set; } = string.Empty;      // e.g. Low / Medium / High

        [Required]
        public int PeopleCount { get; set; }

        [Required]
        public float AiConfidencePercentage { get; set; }

        /// <summary>Base-64 encoded image captured by the camera.</summary>
        [Required]
        public string ImageBase64 { get; set; } = string.Empty;

        /// <summary>UTC timestamp produced by the AI service at capture time.</summary>
        [Required]
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Dashboard response DTO — returned by the GET endpoints.
    /// </summary>
    public class CrowdDensityResultMV
    {
        public int CrowdId { get; set; }
        public string DensityLevel { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int PeopleCount { get; set; }
        public float AiConfidencePercentage { get; set; }
        public DateTime Timestamp { get; set; }
        public int? CameraId { get; set; }
        public int? BinId { get; set; }
    }
}
