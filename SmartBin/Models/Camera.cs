using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBin.Models
{
    [Index(nameof(IdentificationToken), IsUnique = true)]
    public class Camera
    {
        [Key]
        public int CameraId { get; set; }
        public string IdentificationToken { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int CrowdThreshold { get; set; } = 5;
        public int WasteThreshold { get; set; } = 5;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public int? UserId { get; set; }
        public AppUser User { get; set; }

        // Navigation Properties
        public List<CrowdDensity> CrowdHistories { get; set; }
        public List<SurroundingWaste> WasteHistories { get; set; }
    }
}
