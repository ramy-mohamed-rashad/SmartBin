using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;

namespace SmartBin.Services
{
    public interface ICrowdDensityService
    {
        Task<CrowdDensityResultMV?> GetLastStateAsync(int? BinId, int? CameraId, AppUser user);
        Task<List<CrowdDensityResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50);
        Task<(bool success, string message)> ReportCrowdDensityAsync(CrowdDensityReportMV dto, string cameraToken, string binToken, IWebHostEnvironment env);
    }

    public class CrowdDensityService : ICrowdDensityService
    {
        private readonly AppDbContext _context;

        public CrowdDensityService(AppDbContext context)
        {
            _context = context;
        }

        // ── GET: last record ──────────────────────────────────────────────────
        public async Task<CrowdDensityResultMV?> GetLastStateAsync(int? BinId, int? CameraId, AppUser user)
        {
            CrowdDensity? lastState = null;

            if (BinId.HasValue)
            {
                var bin = await _context.Bins.FirstOrDefaultAsync(b => b.BinId == BinId.Value && user.Id == b.UserId);
                if (bin == null) return null;

                lastState = await _context.CrowdDensities
                    .Where(s => s.BinId == BinId.Value)
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefaultAsync();
            }
            else if (CameraId.HasValue)
            {
                var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.CameraId == CameraId.Value && user.Id == c.UserId);
                if (camera == null) return null;

                lastState = await _context.CrowdDensities
                    .Where(s => s.CameraId == CameraId.Value)
                    .OrderByDescending(s => s.Timestamp)
                    .FirstOrDefaultAsync();
            }

            if (lastState == null) return null;
            return MapToResult(lastState);
        }

        // ── GET: history ──────────────────────────────────────────────────────
        public async Task<List<CrowdDensityResultMV>> GetHistoryAsync(int? BinId, int? CameraId, AppUser user, int take = 50)
        {
            IQueryable<CrowdDensity>? query = null;

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
                .Select(s => MapToResult(s))
                .ToListAsync();
        }

        // ── POST: receive AI result from external service ─────────────────────
        public async Task<(bool success, string message)> ReportCrowdDensityAsync(
            CrowdDensityReportMV dto, string cameraToken, string binToken, IWebHostEnvironment env)
        {
            // Resolve camera by its identification token
            var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.IdentificationToken == cameraToken);
            if (camera == null)
                return (false, "Camera not found.");

            // Resolve bin by its identification token
            var bin = await _context.Bins.FirstOrDefaultAsync(b => b.IdentificationToken == binToken);
            if (bin == null)
                return (false, "Bin not found.");

            // Decode and persist image
            string imageUrl;
            try
            {
                imageUrl = await SaveBase64ImageAsync(dto.ImageBase64, "crowd", env);
            }
            catch (Exception ex)
            {
                return (false, $"Image save failed: {ex.Message}");
            }

            var record = new CrowdDensity
            {
                DensityLevel           = dto.DensityLevel,
                PeopleCount            = dto.PeopleCount,
                AiConfidencePercentage = dto.AiConfidencePercentage,
                ImageUrl               = imageUrl,
                Timestamp              = dto.Timestamp,
                CameraId               = camera.CameraId,
                BinId                  = bin.BinId
            };

            _context.CrowdDensities.Add(record);
            await _context.SaveChangesAsync();
            return (true, "Crowd density recorded successfully.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static CrowdDensityResultMV MapToResult(CrowdDensity s) =>
            new CrowdDensityResultMV
            {
                CrowdId                = s.CrowdId,
                DensityLevel           = s.DensityLevel,
                ImageUrl               = s.ImageUrl,
                PeopleCount            = s.PeopleCount,
                AiConfidencePercentage = s.AiConfidencePercentage,
                Timestamp              = s.Timestamp,
                CameraId               = s.CameraId,
                BinId                  = s.BinId
            };

        /// <summary>
        /// Decodes a base-64 string and writes it to wwwroot/uploads/{folder}/.
        /// Returns the public URL path (e.g. "/uploads/crowd/abc123.jpg").
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
