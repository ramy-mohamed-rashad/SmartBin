using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBin.Models
{
    public class BinSection
    {
        [Key]
        public int SectionId { get; set; }
        public float LevelPercentage { get; set; }
        public float Weight { get; set; }
        public DateTime? LastUpdated { get; set; }
        public int Threshold { get; set; } = 80;

        [ForeignKey("Bin")]
        public int BinId { get; set; }
        public Bin Bin { get; set; }

        [ForeignKey("Material")]
        public int MaterialId { get; set; }
        public Material Material { get; set; }
    }
}
