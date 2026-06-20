namespace SmartBin.ModelViews
{
    public class BinRequestInsertMV
    {
        public string RequesterName { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class BinRequestResultMV
    {
        public int Id { get; set; }
        public string RequesterName { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; }
        public int UserId { get; set; }
        public int? BinId { get; set; }
        public string? IdentificationToken { get; set; }
    }
}
