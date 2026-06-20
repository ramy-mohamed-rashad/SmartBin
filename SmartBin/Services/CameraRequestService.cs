using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;

namespace SmartBin.Services
{
    public interface ICameraRequestService
    {
        Task<CameraRequestResultMV> SubmitRequest(int userId, CameraRequestInsertMV model);
        Task<List<CameraRequestResultMV>> GetAllRequests();
        Task<List<CameraRequestResultMV>> GetUserRequests(int userId);
        Task<CameraRequestResultMV?> AcceptRequest(int requestId);
    }

    public class CameraRequestService : ICameraRequestService
    {
        private readonly AppDbContext _context;

        public CameraRequestService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<CameraRequestResultMV> SubmitRequest(int userId, CameraRequestInsertMV model)
        {
            var request = new CameraRequest
            {
                RequesterName = model.RequesterName,
                Latitude = model.Latitude,
                Longitude = model.Longitude,
                Status = "Pending",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.CameraRequests.Add(request);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);
            return MapToResult(request, user, null);
        }

        public async Task<List<CameraRequestResultMV>> GetAllRequests()
        {
            var requests = await _context.CameraRequests
                .Include(r => r.User)
                .Include(r => r.Camera)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return requests.Select(r => MapToResult(r, r.User, r.Camera)).ToList();
        }

        public async Task<List<CameraRequestResultMV>> GetUserRequests(int userId)
        {
            var requests = await _context.CameraRequests
                .Include(r => r.Camera)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var user = await _context.Users.FindAsync(userId);
            return requests.Select(r => MapToResult(r, user, r.Camera)).ToList();
        }

        public async Task<CameraRequestResultMV?> AcceptRequest(int requestId)
        {
            var request = await _context.CameraRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.Status != "Pending")
                return null;

            // Create a new camera and assign it to the requester
            var camera = new Camera
            {
                IdentificationToken = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();

            // Update the request
            request.Status = "Accepted";
            request.CameraId = camera.CameraId;
            await _context.SaveChangesAsync();

            return MapToResult(request, request.User, camera);
        }

        private static CameraRequestResultMV MapToResult(CameraRequest r, AppUser? user, Camera? camera)
        {
            return new CameraRequestResultMV
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
                CameraId = camera?.CameraId,
                IdentificationToken = camera?.IdentificationToken
            };
        }
    }
}
