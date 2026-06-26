using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;
using SmartBin.Services;
namespace SmartBin.Services
{
    public interface IBinService
    {
        public Task<String> CreateBin();
        public Task<String> AssignBin(AppUser User, AssignBinMV Token);
        public Task<BinMV> UpdateBin(BinUpdateMV bin, string IdentificationToken);
        public Task<bool> DeleteBin(string IdentificationToken);
        public Task<BinDetailsMV> GetBin(int id, AppUser user);
        public Task<List<BinMV>> GetAllBins(AppUser user);
        public Task<bool> UpdateSettingsAsync(int id, BinSettingsUpdateMV settings, AppUser user);

    }
    public class BinService : IBinService
    {

        private readonly AppDbContext _context;
        private readonly IBinSectionService _sectionService;
        public BinService(AppDbContext context, IBinSectionService sectionService)
        {
            _context = context;
            _sectionService = sectionService;
        }

        public async Task<String> CreateBin()
        {
            var bin = new Bin
            {
                IdentificationToken = Guid.NewGuid().ToString()
            };
            _context.Bins.Add(bin);
            await _context.SaveChangesAsync();
            foreach (var material in await _context.Materials.ToListAsync())
            {
                await _sectionService.CreateBinSection(bin.BinId, material.MaterialId);
            }
            return bin.IdentificationToken;
        }

        public async Task<String> AssignBin(AppUser User, AssignBinMV Token)
        {
            var bin = await _context.Bins.FirstOrDefaultAsync(b => b.IdentificationToken == Token.token);
            if (bin == null) return null;
            bin.UserId = User.Id;
            await _context.SaveChangesAsync();
            return bin.IdentificationToken;
        }

        public async Task<BinMV> UpdateBin(BinUpdateMV bin, string IdentificationToken)
        {
            var existingBin = await _context.Bins.FirstOrDefaultAsync(b => b.IdentificationToken == IdentificationToken);
            if (existingBin == null) return null;
            existingBin.Latitude = bin.Latitude;
            existingBin.Longitude = bin.Longitude;
            await _context.SaveChangesAsync();
            return new BinMV
            {
                BinId = existingBin.BinId,
                Latitude = existingBin.Latitude,
                Longitude = existingBin.Longitude
            };
        }

        public async Task<bool> DeleteBin(string IdentificationToken)
        {
            var existingBin = await _context.Bins.FirstOrDefaultAsync(b => b.IdentificationToken == IdentificationToken);
            if (existingBin == null) return false;
            _context.Bins.Remove(existingBin);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BinDetailsMV> GetBin(int id, AppUser user)
        {
            var bin = await _context.Bins.FirstOrDefaultAsync(b => b.BinId == id && b.UserId == user.Id);
            if (bin == null) return null;
            var sections = await _context.BinSections.Include(s => s.Material).Where(s => s.BinId == id).ToListAsync();

            int? associatedCameraId = null;
            if (!string.IsNullOrEmpty(bin.CameraToken))
            {
                var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.IdentificationToken == bin.CameraToken);
                if (camera != null)
                {
                    associatedCameraId = camera.CameraId;
                }
            }

            return new BinDetailsMV
            {
                BinId = bin.BinId,
                Latitude = bin.Latitude,
                Longitude = bin.Longitude,
                FillThreshold = bin.FillThreshold,
                CameraToken = bin.CameraToken,
                AssociatedCameraId = associatedCameraId,
                Sections = sections.Select(s => new BinSectionMV
                {
                    MaterialName = s.Material.Name,
                    LevelPercentage = s.LevelPercentage,
                    Weight = s.Weight,
                    Threshold = s.Threshold
                }).ToList()
            };
        }

        public async Task<bool> UpdateSettingsAsync(int id, BinSettingsUpdateMV settings, AppUser user)
        {
            var bin = await _context.Bins.FirstOrDefaultAsync(b => b.BinId == id && b.UserId == user.Id);
            if (bin == null) return false;
            bin.FillThreshold = settings.FillThreshold;
            bin.CameraToken = settings.CameraToken;

            // Update individual section thresholds via a keyword → value map:
            var thresholdMap = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase)
            {
                { "metal",   settings.MetalThreshold },
                { "plastic", settings.PlasticThreshold },
                { "paper",   settings.PaperThreshold },
                { "organic", settings.OrganicThreshold },
                { "glass",   settings.GlassThreshold }
            };

            var sections = await _context.BinSections.Include(s => s.Material).Where(s => s.BinId == id).ToListAsync();
            foreach (var sec in sections)
            {
                var matName = sec.Material.Name.ToLower();
                foreach (var kv in thresholdMap)
                {
                    if (matName.Contains(kv.Key) && kv.Value.HasValue)
                    {
                        sec.Threshold = kv.Value.Value;
                        break;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<List<BinMV>> GetAllBins(AppUser user)
        {
            var bins = await _context.Bins.Where(b => b.UserId == user.Id).ToListAsync();
            return bins.Select(bin => new BinMV
            {
                BinId = bin.BinId,
                Latitude = bin.Latitude,
                Longitude = bin.Longitude
            }).ToList();

        }
    }
}
