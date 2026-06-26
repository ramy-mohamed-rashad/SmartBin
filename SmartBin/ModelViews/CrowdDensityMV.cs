using Microsoft.AspNetCore.Http;

namespace SmartBin.ModelViews
{
    public class CrowdDensityInsertMV
    {
        public IFormFile? Image { get; set; }
        public string? DensityLevel { get; set; }
        public int? PeopleCount { get; set; }
        public float? AiConfidencePercentage { get; set; }
    }
    public class CrowdDensityResultMV
    {
        public string DensityLevel { get; set; } // e.g., Low, Medium, High
        public int PeopleCount { get; set; }
        public float AiConfidencePercentage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
