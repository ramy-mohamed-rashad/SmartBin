using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartBin.Services
{
    public interface ISurroundingWasteService
    {
        public Task<SurroundingWasteResultMV> GetLastStateAsync(int? BinId, int? CameraId, AppUser user);
        public Task<List<SurroundingWasteResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50);
        public Task<string> AddSurroundingWasteAsync(SurroundingWasteInsertMV waste, string token);
    }

    public class SurroundingWasteService : ISurroundingWasteService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public SurroundingWasteService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
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

        public async Task<string> AddSurroundingWasteAsync(SurroundingWasteInsertMV waste, string token)
        {
            var bin = await _context.Bins.FirstOrDefaultAsync(b => b.IdentificationToken == token);
            if (bin != null)
            {
                var record = new SurroundingWaste
                {
                    WasteLevel = waste.WasteLevel ?? "Low",
                    DetectedObjectsCount = waste.DetectedObjectsCount ?? 1,
                    AiConfidencePercentage = waste.AiConfidencePercentage ?? 90.0f,
                    ImageUrl = "mocked_waste.jpg",
                    Timestamp = DateTime.UtcNow,
                    BinId = bin.BinId
                };
                _context.SurroundingWastes.Add(record);
                await _context.SaveChangesAsync();

                if (record.WasteLevel == "High" && bin.UserId.HasValue)
                {
                    await _notificationService.CreateNotificationAsync(
                        bin.UserId.Value,
                        "High Surrounding Waste Alert",
                        $"High volume of surrounding waste detected near Bin #{bin.BinId} ({record.DetectedObjectsCount} objects).",
                        "Waste",
                        binId: bin.BinId
                    );
                }

                return "Surrounding waste added for Bin";
            }

            var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.IdentificationToken == token);
            if (camera != null)
            {
                var record = new SurroundingWaste
                {
                    WasteLevel = waste.WasteLevel ?? "Low",
                    DetectedObjectsCount = waste.DetectedObjectsCount ?? 1,
                    AiConfidencePercentage = waste.AiConfidencePercentage ?? 90.0f,
                    ImageUrl = "mocked_waste.jpg",
                    Timestamp = DateTime.UtcNow,
                    CameraId = camera.CameraId
                };
                _context.SurroundingWastes.Add(record);
                await _context.SaveChangesAsync();

                if (camera.UserId.HasValue && (record.DetectedObjectsCount > camera.WasteThreshold || record.WasteLevel == "High"))
                {
                    var connectedBin = await _context.Bins.FirstOrDefaultAsync(b => b.CameraToken == camera.IdentificationToken);
                    int? binIdToNotify = connectedBin?.BinId;

                    await _notificationService.CreateNotificationAsync(
                        camera.UserId.Value,
                        "High Surrounding Waste Alert",
                        $"Surrounding waste objects count near Camera #{camera.CameraId} reached {record.DetectedObjectsCount} (exceeding your threshold of {camera.WasteThreshold} or classified as High).",
                        "Waste",
                        binId: binIdToNotify,
                        cameraId: camera.CameraId
                    );
                }

                return "Surrounding waste added for Camera";
            }

            return "Bin not found";
        }
    }
}
