using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBin.Models
{
    public class CameraRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string RequesterName { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        /// <summary>"Pending" | "Accepted" | "Rejected"</summary>
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public int UserId { get; set; }
        public AppUser User { get; set; }

        // Set after admin accepts the request
        [ForeignKey("Camera")]
        public int? CameraId { get; set; }
        public Camera? Camera { get; set; }
    }
}
