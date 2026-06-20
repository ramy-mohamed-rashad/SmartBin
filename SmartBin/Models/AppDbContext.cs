using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace SmartBin.Models
{
    public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<int>, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Bin> Bins { get; set; }
        public DbSet<BinSection> BinSections { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<SurroundingWaste> SurroundingWastes { get; set; }
        public DbSet<CrowdDensity> CrowdDensities { get; set; }
        public DbSet<BinRequest> BinRequests { get; set; }
        public DbSet<Camera> Cameras { get; set; }
        public DbSet<CameraRequest> CameraRequests { get; set; }

    }
}
