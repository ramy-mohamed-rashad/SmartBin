using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;

namespace SmartBin.Services
{
    public interface ICameraService
    {
        public Task<string> CreateCamera();
        public Task<string> AssignCamera(AppUser user, AssignBinMV token); // Reusing AssignBinMV since it's just a token wrapper
        public Task<CameraMV> UpdateCamera(CameraUpdateMV camera, string identificationToken);
        public Task<bool> DeleteCamera(string identificationToken);
        public Task<CameraDetailsMV> GetCamera(int id, AppUser user);
        public Task<List<CameraMV>> GetAllCameras(AppUser user);
    }

    public class CameraService : ICameraService
    {
        private readonly AppDbContext _context;

        public CameraService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateCamera()
        {
            var camera = new Camera
            {
                IdentificationToken = Guid.NewGuid().ToString()
            };
            _context.Cameras.Add(camera);
            await _context.SaveChangesAsync();
            return camera.IdentificationToken;
        }

        public async Task<string> AssignCamera(AppUser user, AssignBinMV token)
        {
            var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.IdentificationToken == token.token);
            if (camera == null) return null;
            camera.UserId = user.Id;
            await _context.SaveChangesAsync();
            return camera.IdentificationToken;
        }

        public async Task<CameraMV> UpdateCamera(CameraUpdateMV camera, string identificationToken)
        {
            var existingCamera = await _context.Cameras.FirstOrDefaultAsync(c => c.IdentificationToken == identificationToken);
            if (existingCamera == null) return null;
            existingCamera.Latitude = camera.Latitude;
            existingCamera.Longitude = camera.Longitude;
            await _context.SaveChangesAsync();
            return new CameraMV
            {
                CameraId = existingCamera.CameraId,
                Latitude = existingCamera.Latitude,
                Longitude = existingCamera.Longitude
            };
        }

        public async Task<bool> DeleteCamera(string identificationToken)
        {
            var existingCamera = await _context.Cameras.FirstOrDefaultAsync(c => c.IdentificationToken == identificationToken);
            if (existingCamera == null) return false;
            _context.Cameras.Remove(existingCamera);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CameraDetailsMV> GetCamera(int id, AppUser user)
        {
            var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.CameraId == id && c.UserId == user.Id);
            if (camera == null) return null;
            return new CameraDetailsMV
            {
                CameraId = camera.CameraId,
                Latitude = camera.Latitude,
                Longitude = camera.Longitude
            };
        }

        public async Task<List<CameraMV>> GetAllCameras(AppUser user)
        {
            var cameras = await _context.Cameras.Where(c => c.UserId == user.Id).ToListAsync();
            return cameras.Select(camera => new CameraMV
            {
                CameraId = camera.CameraId,
                Latitude = camera.Latitude,
                Longitude = camera.Longitude
            }).ToList();
        }
    }
}
