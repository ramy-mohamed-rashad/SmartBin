using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;

namespace SmartBin.Services
{
    public interface IBinRequestService
    {
        Task<BinRequestResultMV> SubmitRequest(int userId, BinRequestInsertMV model);
        Task<List<BinRequestResultMV>> GetAllRequests();
        Task<List<BinRequestResultMV>> GetUserRequests(int userId);
        Task<BinRequestResultMV?> AcceptRequest(int requestId);
    }

    public class BinRequestService : IBinRequestService
    {
        private readonly AppDbContext _context;
        private readonly IBinService _binService;
        private readonly IBinSectionService _sectionService;

        public BinRequestService(AppDbContext context, IBinService binService, IBinSectionService sectionService)
        {
            _context = context;
            _binService = binService;
            _sectionService = sectionService;
        }

        public async Task<BinRequestResultMV> SubmitRequest(int userId, BinRequestInsertMV model)
        {
            var request = new BinRequest
            {
                RequesterName = model.RequesterName,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Status = "Pending",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.BinRequests.Add(request);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return MapToResult(request, user, null);
        }

        public async Task<List<BinRequestResultMV>> GetAllRequests()
        {
            var requests = await _context.BinRequests
                .Include(r => r.User)
                .Include(r => r.Bin)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(r => MapToResult(r, r.User, r.Bin)).ToList();
        }

        public async Task<List<BinRequestResultMV>> GetUserRequests(int userId)
        {
            var requests = await _context.BinRequests
                .Include(r => r.Bin)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var user = await _context.Users.FindAsync(userId);
            return requests.Select(r => MapToResult(r, user, r.Bin)).ToList();
        }

        public async Task<BinRequestResultMV?> AcceptRequest(int requestId)
        {
            var request = await _context.BinRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.Status != "Pending")
                return null;

            // Create a new bin and assign it to the requester
            var bin = new Bin
            {
                IdentificationToken = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            _context.Bins.Add(bin);
            await _context.SaveChangesAsync();

            // Create sections for all materials
            var materials = await _context.Materials.ToListAsync();
            foreach (var material in materials)
            {
                await _sectionService.CreateBinSection(bin.BinId, material.MaterialId);
            }

            // Update the request
            request.Status = "Accepted";
            request.BinId = bin.BinId;
            await _context.SaveChangesAsync();

            return MapToResult(request, request.User, bin);
        }

        private static BinRequestResultMV MapToResult(BinRequest r, AppUser? user, Bin? bin)
        {
            return new BinRequestResultMV
            {
                Id = r.Id,
                RequesterName = r.RequesterName,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                UserEmail = user?.Email ?? "",
                UserFullName = user?.FullName ?? "",
                UserId = r.UserId,
                BinId = bin?.BinId,
                IdentificationToken = bin?.IdentificationToken
            };
        }
    }
}
