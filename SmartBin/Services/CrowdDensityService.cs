using SmartBin.Models;
using SmartBin.ModelViews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartBin.Services
{
    public interface ICrowdDensityService
    {
        public Task<CrowdDensityResultMV> GetLastStateAsync(int? BinId, int? CameraId, AppUser user);
        public Task<List<CrowdDensityResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50);
        public Task<string> AddCrowdDensityAsync(CrowdDensityInsertMV crowdDensity, string token);
    }

    public class CrowdDensityService : ICrowdDensityService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public CrowdDensityService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

        public async Task<List<CrowdDensityResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50)
        {
            IQueryable<CrowdDensity> query = null;
            if (BinId.HasValue)
            {
                var bin = await _context.Bins.FirstOrDefaultAsync(b => b.BinId == BinId.Value && user.Id == b.UserId);
                if (bin == null) return new List<CrowdDensityResultMV>();
                query = _context.CrowdDensities.Where(s => s.BinId == BinId.Value);
            }
            else if (CameraId.HasValue)
            {
                var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.CameraId == CameraId.Value && user.Id == c.UserId);
                if (camera == null) return new List<CrowdDensityResultMV>();
                query = _context.CrowdDensities.Where(s => s.CameraId == CameraId.Value);
            }
            else
            {
                return new List<CrowdDensityResultMV>();
            }

            return await query
                .OrderByDescending(s => s.Timestamp)
                .Take(take)
                .Select(s => new CrowdDensityResultMV
                {
                    DensityLevel = s.DensityLevel,
                    PeopleCount = s.PeopleCount,
                    AiConfidencePercentage = s.AiConfidencePercentage,
                    Timestamp = s.Timestamp
                })
                .ToListAsync();
        }

        public async Task<string> AddCrowdDensityAsync(CrowdDensityInsertMV crowdDensity, string token)
        {
            var bin = await _context.Bins.FirstOrDefaultAsync(b => b.IdentificationToken == token);
            if (bin != null)
            {
                var record = new CrowdDensity
                {
                    DensityLevel = crowdDensity.DensityLevel ?? "Low",
                    PeopleCount = crowdDensity.PeopleCount ?? 2,
                    AiConfidencePercentage = crowdDensity.AiConfidencePercentage ?? 85.0f,
                    ImageUrl = "mocked_image.jpg",
                    Timestamp = DateTime.UtcNow,
                    BinId = bin.BinId
                };
                _context.CrowdDensities.Add(record);
                await _context.SaveChangesAsync();

                if (record.DensityLevel == "High" && bin.UserId.HasValue)
                {
                    await _notificationService.CreateNotificationAsync(
                        bin.UserId.Value,
                        "High Crowd Alert",
                        $"High crowd detected near Bin #{bin.BinId} ({record.PeopleCount} people).",
                        "Crowd",
                        binId: bin.BinId
                    );
                }

                return "Crowd density added for Bin";
            }

            var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.IdentificationToken == token);
            if (camera != null)
            {
                var record = new CrowdDensity
                {
                    DensityLevel = crowdDensity.DensityLevel ?? "Low",
                    PeopleCount = crowdDensity.PeopleCount ?? 2,
                    AiConfidencePercentage = crowdDensity.AiConfidencePercentage ?? 85.0f,
                    ImageUrl = "mocked_image.jpg",
                    Timestamp = DateTime.UtcNow,
                    CameraId = camera.CameraId
                };
                _context.CrowdDensities.Add(record);
                await _context.SaveChangesAsync();

                if (camera.UserId.HasValue && (record.PeopleCount > camera.CrowdThreshold || record.DensityLevel == "High"))
                {
                    var connectedBin = await _context.Bins.FirstOrDefaultAsync(b => b.CameraToken == camera.IdentificationToken);
                    int? binIdToNotify = connectedBin?.BinId;

                    await _notificationService.CreateNotificationAsync(
                        camera.UserId.Value,
                        "High Crowd Alert",
                        $"Crowd count near Camera #{camera.CameraId} reached {record.PeopleCount} people (exceeding your threshold of {camera.CrowdThreshold} or classified as High).",
                        "Crowd",
                        binId: binIdToNotify,
                        cameraId: camera.CameraId
                    );
                }

                return "Crowd density added for Camera";
            }

            return "Bin not found";
        }
    }
}
