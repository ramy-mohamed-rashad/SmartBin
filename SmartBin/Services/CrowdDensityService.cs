using SmartBin.Models;
using SmartBin.ModelViews;
using Microsoft.EntityFrameworkCore;

namespace SmartBin.Services
{
    public interface ICrowdDensityService
    {
        public Task<CrowdDensityResultMV> GetLastStateAsync(int? BinId, int? CameraId, AppUser user);
        public string AddCrowdDensityAsync(CrowdDensityInsertMV crowdDensity, string token);
    }

    public class CrowdDensityService : ICrowdDensityService
    {
        private readonly AppDbContext _context;

        public CrowdDensityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CrowdDensityResultMV> GetLastStateAsync(int? BinId, int? CameraId, AppUser user)
        {
            if (BinId.HasValue)
            {
                var bin = await _context.Bins.FirstOrDefaultAsync(b => b.BinId == BinId.Value && user.Id == b.UserId);
                if (bin == null)
                    return null;
                var LastState = await _context.CrowdDensities.Where(s => s.BinId == BinId.Value).OrderByDescending(s => s.Timestamp).FirstOrDefaultAsync();
                if (LastState == null)
                    return null;
                return new CrowdDensityResultMV
                {
                    DensityLevel = LastState.DensityLevel,
                    PeopleCount = LastState.PeopleCount,
                    AiConfidencePercentage = LastState.AiConfidencePercentage,
                    Timestamp = LastState.Timestamp
                };
            }
            else if (CameraId.HasValue)
            {
                var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.CameraId == CameraId.Value && user.Id == c.UserId);
                if (camera == null)
                    return null;
                var LastState = await _context.CrowdDensities.Where(s => s.CameraId == CameraId.Value).OrderByDescending(s => s.Timestamp).FirstOrDefaultAsync();
                if (LastState == null)
                    return null;
                return new CrowdDensityResultMV
                {
                    DensityLevel = LastState.DensityLevel,
                    PeopleCount = LastState.PeopleCount,
                    AiConfidencePercentage = LastState.AiConfidencePercentage,
                    Timestamp = LastState.Timestamp
                };
            }
            return null;
        }

        public string AddCrowdDensityAsync(CrowdDensityInsertMV crowdDensity, string token)
        {
            var bin = _context.Bins.FirstOrDefault(b => b.IdentificationToken == token);
            if (bin != null)
            {
                var record = new CrowdDensity
                {
                    DensityLevel = "Low",
                    PeopleCount = 2,
                    AiConfidencePercentage = 85.0f,
                    ImageUrl = "mocked_image.jpg",
                    Timestamp = DateTime.UtcNow,
                    BinId = bin.BinId
                };
                _context.CrowdDensities.Add(record);
                _context.SaveChanges();
                return "Crowd density added for Bin";
            }

            var camera = _context.Cameras.FirstOrDefault(c => c.IdentificationToken == token);
            if (camera != null)
            {
                var record = new CrowdDensity
                {
                    DensityLevel = "Low",
                    PeopleCount = 2,
                    AiConfidencePercentage = 85.0f,
                    ImageUrl = "mocked_image.jpg",
                    Timestamp = DateTime.UtcNow,
                    CameraId = camera.CameraId
                };
                _context.CrowdDensities.Add(record);
                _context.SaveChanges();
                return "Crowd density added for Camera";
            }

            return "Bin not found";
        }
    }
}
