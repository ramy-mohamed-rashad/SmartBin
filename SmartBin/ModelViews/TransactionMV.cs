using System.ComponentModel.DataAnnotations;

namespace SmartBin.ModelViews
{
    public class TransactionResultMV
    {
        public int TransactionId { get; set; }
        public DateTime Timestamp { get; set; }
        public float AiConfidencePercentage { get; set; } = 0;
        public string? ImageUrl { get; set; }
        public int BinId { get; set; }
        public string MaterialName { get; set; }
    }
    public class TransactionInsertMV : IValidatableObject
    {
        public string? MaterialName { get; set; }
        public IFormFile? Image { get; set; }
        public int? Weight { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool hasMaterial = !string.IsNullOrWhiteSpace(MaterialName);
            bool hasImage = Image != null && Image.Length > 0;
            if (!hasMaterial && !hasImage)
            {
                yield return new ValidationResult(
                    "You must provide either a MaterialName or an Image. Both cannot be empty.",
                    new[] { nameof(MaterialName), nameof(Image) }
                );
            }
        }
    }

}
