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
        // We will return empty lists if no history, or map them as list MV
    }

    public class CameraUpdateMV
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
}
