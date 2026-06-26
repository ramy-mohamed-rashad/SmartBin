using System;

namespace SmartBin.ModelViews
{
    public class NotificationResultMV
    {
        public int NotificationId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; } // "Crowd", "Waste", "BinFill"
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? DeviceId { get; set; }
        public string? DeviceType { get; set; } // "Bin" or "Camera"
    }
}
