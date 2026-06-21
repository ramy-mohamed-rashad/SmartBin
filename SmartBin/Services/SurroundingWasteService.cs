using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;

namespace SmartBin.Services
{
    public interface ISurroundingWasteService
    {
        public Task<SurroundingWasteResultMV> GetLastStateAsync(int? BinId, int? CameraId, AppUser user);
        public Task<List<SurroundingWasteResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50);
        public string AddSurroundingWasteAsync(SurroundingWasteInsertMV waste, string token);
    }

    public class SurroundingWasteService : ISurroundingWasteService
    {
        private readonly AppDbContext _context;

        public SurroundingWasteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SurroundingWasteResultMV> GetLastStateAsync(int? BinId, int? CameraId, AppUser user)
        {
            if (BinId.HasValue)
            {
                var bin = await _context.Bins.FirstOrDefaultAsync(b => b.BinId == BinId.Value && user.Id == b.UserId);
                if (bin == null)
                    return null;
                var LastState = await _context.SurroundingWastes.Where(s => s.BinId == BinId.Value).OrderByDescending(s => s.Timestamp).FirstOrDefaultAsync();
                if (LastState == null)
                    return null;
                return new SurroundingWasteResultMV
                {
                    Id = LastState.Id,
                    WasteLevel = LastState.WasteLevel,
                    ImageUrl = LastState.ImageUrl,
                    DetectedObjectsCount = LastState.DetectedObjectsCount,
                    AiConfidencePercentage = LastState.AiConfidencePercentage,
                    Timestamp = LastState.Timestamp
                };
            }
            else if (CameraId.HasValue)
            {
                var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.CameraId == CameraId.Value && user.Id == c.UserId);
                if (camera == null)
                    return null;
                var LastState = await _context.SurroundingWastes.Where(s => s.CameraId == CameraId.Value).OrderByDescending(s => s.Timestamp).FirstOrDefaultAsync();
                if (LastState == null)
                    return null;
                return new SurroundingWasteResultMV
                {
                    Id = LastState.Id,
                    WasteLevel = LastState.WasteLevel,
                    ImageUrl = LastState.ImageUrl,
                    DetectedObjectsCount = LastState.DetectedObjectsCount,
                    AiConfidencePercentage = LastState.AiConfidencePercentage,
                    Timestamp = LastState.Timestamp
                };
            }
            return null;
        }

        public async Task<List<SurroundingWasteResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50)
        {
            IQueryable<SurroundingWaste> query = null;
            if (BinId.HasValue)
            {
                var bin = await _context.Bins.FirstOrDefaultAsync(b => b.BinId == BinId.Value && user.Id == b.UserId);
                if (bin == null) return new List<SurroundingWasteResultMV>();
                query = _context.SurroundingWastes.Where(s => s.BinId == BinId.Value);
            }
            else if (CameraId.HasValue)
            {
                var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.CameraId == CameraId.Value && user.Id == c.UserId);
                if (camera == null) return new List<SurroundingWasteResultMV>();
                query = _context.SurroundingWastes.Where(s => s.CameraId == CameraId.Value);
            }
            else
            {
                return new List<SurroundingWasteResultMV>();
            }

            return await query
                .OrderByDescending(s => s.Timestamp)
                .Take(take)
                .Select(s => new SurroundingWasteResultMV
                {
                    Id = s.Id,
                    WasteLevel = s.WasteLevel,
                    ImageUrl = s.ImageUrl,
                    DetectedObjectsCount = s.DetectedObjectsCount,
                    AiConfidencePercentage = s.AiConfidencePercentage,
                    Timestamp = s.Timestamp
                })
                .ToListAsync();
        }

        public string AddSurroundingWasteAsync(SurroundingWasteInsertMV waste, string token)
        {
            var bin = _context.Bins.FirstOrDefault(b => b.IdentificationToken == token);
            if (bin != null)
            {
                var record = new SurroundingWaste
                {
                    WasteLevel = "Low",
                    DetectedObjectsCount = 1,
                    AiConfidencePercentage = 90.0f,
                    ImageUrl = "mocked_waste.jpg",
                    Timestamp = DateTime.UtcNow,
                    BinId = bin.BinId
                };
                _context.SurroundingWastes.Add(record);
                _context.SaveChanges();
                return "Surrounding waste added for Bin";
            }

            var camera = _context.Cameras.FirstOrDefault(c => c.IdentificationToken == token);
            if (camera != null)
            {
                var record = new SurroundingWaste
                {
                    WasteLevel = "Low",
                    DetectedObjectsCount = 1,
                    AiConfidencePercentage = 90.0f,
                    ImageUrl = "mocked_waste.jpg",
                    Timestamp = DateTime.UtcNow,
                    CameraId = camera.CameraId
                };
                _context.SurroundingWastes.Add(record);
                _context.SaveChanges();
                return "Surrounding waste added for Camera";
            }

            return "Bin not found";
        }
    }
}
