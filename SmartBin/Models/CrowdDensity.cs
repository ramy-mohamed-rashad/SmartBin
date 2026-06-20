using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBin.Models
{
    public class CrowdDensity
    {
        [Key]
        public int CrowdId { get; set; }
        public string DensityLevel { get; set; } // e.g., Low, Medium, High
        public string ImageUrl { get; set; }
        public int PeopleCount { get; set; }
        public float AiConfidencePercentage { get; set; }
        public DateTime Timestamp { get; set; }
        [ForeignKey("Bin")]
        public int? BinId { get; set; }
        public Bin? Bin { get; set; }

        [ForeignKey("Camera")]
        public int? CameraId { get; set; }
        public Camera? Camera { get; set; }
    }
}
