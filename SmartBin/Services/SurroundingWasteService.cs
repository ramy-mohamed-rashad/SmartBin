using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;

namespace SmartBin.Services
{
    public interface ISurroundingWasteService
    {
        Task<SurroundingWasteResultMV?> GetLastStateAsync(int? BinId, int? CameraId, AppUser user);
        Task<List<SurroundingWasteResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50);
        Task<(bool success, string message)> ReportSurroundingWasteAsync(SurroundingWasteReportMV dto, string cameraToken, IWebHostEnvironment env);
    }

    public class SurroundingWasteService : ISurroundingWasteService
    {
        private readonly AppDbContext _context;

        public SurroundingWasteService(AppDbContext context)
        {
            _context = context;
        }

        // ── GET: last record ──────────────────────────────────────────────────
        public async Task<SurroundingWasteResultMV?> GetLastStateAsync(int? BinId, int? CameraId, AppUser user)
        {
            SurroundingWaste? lastState = null;

            if (BinId.HasValue)
            {
                var bin = await _context.Bins.FirstOrDefaultAsync(b => b.BinId == BinId.Value && user.Id == b.UserId);
                if (bin == null) return null;

                lastState = await _context.SurroundingWastes
                    .Where(s => s.BinId == BinId.Value)
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefaultAsync();
            }
            else if (CameraId.HasValue)
            {
                var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.CameraId == CameraId.Value && user.Id == c.UserId);
                if (camera == null) return null;

                lastState = await _context.SurroundingWastes
                    .Where(s => s.CameraId == CameraId.Value)
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefaultAsync();
            }

            if (lastState == null) return null;
            return MapToResult(lastState);
        }

        // ── GET: history ──────────────────────────────────────────────────────
        public async Task<List<SurroundingWasteResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50)
        {
            IQueryable<SurroundingWaste>? query = null;

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
                .Select(s => MapToResult(s))
                .ToListAsync();
        }

        // ── POST: receive AI result from external service ─────────────────────
        public async Task<(bool success, string message)> ReportSurroundingWasteAsync(
            SurroundingWasteReportMV dto, string cameraToken, IWebHostEnvironment env)
        {
            // Resolve camera by its identification token
            var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.IdentificationToken == cameraToken);
            if (camera == null)
                return (false, "Camera not found.");

            // Decode and persist image
            string imageUrl;
            try
            {
                imageUrl = await SaveBase64ImageAsync(dto.ImageBase64, "waste", env);
            }
            catch (Exception ex)
            {
                return (false, $"Image save failed: {ex.Message}");
            }

            var record = new SurroundingWaste
            {
                WasteLevel            = dto.WasteLevel,
                DetectedObjectsCount  = dto.DetectedObjectsCount,
                AiConfidencePercentage = dto.AiConfidencePercentage,
                ImageUrl              = imageUrl,
                Timestamp             = dto.Timestamp,
                CameraId              = camera.CameraId
            };

            _context.SurroundingWastes.Add(record);
            await _context.SaveChangesAsync();
            return (true, "Surrounding waste recorded successfully.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static SurroundingWasteResultMV MapToResult(SurroundingWaste s) =>
            new SurroundingWasteResultMV
            {
                Id                    = s.Id,
                WasteLevel            = s.WasteLevel,
                ImageUrl              = s.ImageUrl,
                DetectedObjectsCount  = s.DetectedObjectsCount,
                AiConfidencePercentage = s.AiConfidencePercentage,
                Timestamp             = s.Timestamp,
                CameraId              = s.CameraId
            };

        /// <summary>
        /// Decodes a base-64 string and writes it to wwwroot/uploads/{folder}/.
        /// Returns the public URL path (e.g. "/uploads/waste/abc123.jpg").
        /// </summary>
        private static async Task<string> SaveBase64ImageAsync(string base64, string folder, IWebHostEnvironment env)
        {
            // Strip optional data URI prefix: "data:image/jpeg;base64,..."
            var commaIndex = base64.IndexOf(',');
            var pureBase64 = commaIndex >= 0 ? base64[(commaIndex + 1)..] : base64;

            var bytes = Convert.FromBase64String(pureBase64);
            var uploadDir = Path.Combine(env.WebRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(uploadDir, fileName);
            await File.WriteAllBytesAsync(filePath, bytes);

            return $"/uploads/{folder}/{fileName}";
        }
    }
}
