using System.ComponentModel.DataAnnotations;

namespace SmartBin.ModelViews
{
    /// <summary>
    /// Payload sent by the external AI service to report a surrounding waste reading.
    /// </summary>
    public class SurroundingWasteReportMV
    {
        [Required]
        public string WasteLevel { get; set; } = string.Empty;         // e.g. Low / Medium / High

        [Required]
        public int DetectedObjectsCount { get; set; }

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
    public class SurroundingWasteResultMV
    {
        public int Id { get; set; }
        public string WasteLevel { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int DetectedObjectsCount { get; set; }
        public float AiConfidencePercentage { get; set; }
        public DateTime Timestamp { get; set; }
        public int? CameraId { get; set; }
    }
}
