namespace SmartBin.ModelViews
{
    public class CameraMV
    {
        public int CameraId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CameraDetailsMV
    {
        public int CameraId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int CrowdThreshold { get; set; }
        public int WasteThreshold { get; set; }
    }

    public class CameraUpdateMV
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class CameraSettingsUpdateMV
    {
        public int CrowdThreshold { get; set; }
        public int WasteThreshold { get; set; }
    }
}
