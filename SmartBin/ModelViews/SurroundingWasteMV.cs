using Microsoft.AspNetCore.Http;

namespace SmartBin.ModelViews
{
    public class SurroundingWasteInsertMV
    {
        public IFormFile? Image { get; set; }
        public string? WasteLevel { get; set; }
        public int? DetectedObjectsCount { get; set; }
        public float? AiConfidencePercentage { get; set; }
    }

    public class SurroundingWasteResultMV
    {
        public int Id { get; set; }
        public string WasteLevel { get; set; }
        public string ImageUrl { get; set; }
        public int DetectedObjectsCount { get; set; }
        public float AiConfidencePercentage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
