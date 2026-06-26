namespace SmartBin.ModelViews
{
    public class BinMV
    {
        public int BinId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
    public class BinDetailsMV
    {
        public int BinId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int FillThreshold { get; set; }
        public string? CameraToken { get; set; }
        public int? AssociatedCameraId { get; set; }
        public List<BinSectionMV> Sections { get; set; }
    }
    public class BinUpdateMV { 
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
    public class BinSettingsUpdateMV
    {
        public int FillThreshold { get; set; }
        public string? CameraToken { get; set; }
        public int? MetalThreshold { get; set; }
        public int? PlasticThreshold { get; set; }
        public int? PaperThreshold { get; set; }
        public int? OrganicThreshold { get; set; }
        public int? GlassThreshold { get; set; }
    }
}
