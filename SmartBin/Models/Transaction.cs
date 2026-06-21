using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBin.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public DateTime Timestamp { get; set; }
        public float AiConfidencePercentage { get; set; }
        public int? Weight { get; set; }
        public string? ImageUrl { get; set; }
        [ForeignKey("Bin")]
        public int BinId { get; set; }
        public Bin Bin { get; set; }
        [ForeignKey("Material")]
        public int MaterialId { get; set; }
        public Material Material { get; set; }
    }
}