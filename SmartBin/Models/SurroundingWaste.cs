using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBin.Models
{
    public class SurroundingWaste
    {
        [Key]
        public int Id { get; set; }
        public string WasteLevel { get; set; }
        public string ImageUrl { get; set; }
        public int DetectedObjectsCount { get; set; }
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
