using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBin.Models
{
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [Required]
        [StringLength(50)]
        public string Type { get; set; } // "Crowd", "Waste", "BinFill"

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("User")]
        public int UserId { get; set; }
        public AppUser User { get; set; }

        [ForeignKey("Bin")]
        public int? BinId { get; set; }
        public Bin? Bin { get; set; }

        [ForeignKey("Camera")]
        public int? CameraId { get; set; }
        public Camera? Camera { get; set; }
    }
}
