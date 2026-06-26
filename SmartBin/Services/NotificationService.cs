using Microsoft.EntityFrameworkCore;
using SmartBin.Models;
using SmartBin.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartBin.Services
{
    public interface INotificationService
    {
        Task<List<NotificationResultMV>> GetNotificationsAsync(AppUser user);
        Task<bool> MarkAsReadAsync(int notificationId, AppUser user);
        Task<bool> MarkAllAsReadAsync(AppUser user);
        Task<bool> CreateNotificationAsync(int userId, string title, string message, string type, int? binId = null, int? cameraId = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationResultMV>> GetNotificationsAsync(AppUser user)
        {
            return await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResultMV
                {
                    NotificationId = n.NotificationId,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    DeviceId = n.BinId ?? n.CameraId,
                    DeviceType = n.BinId.HasValue ? "Bin" : (n.CameraId.HasValue ? "Camera" : null)
                })
                .ToListAsync();
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, AppUser user)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == user.Id);

            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(AppUser user)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == user.Id && !n.IsRead)
                .ToListAsync();

            if (!unread.Any()) return true;

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateNotificationAsync(int userId, string title, string message, string type, int? binId = null, int? cameraId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                BinId = binId,
                CameraId = cameraId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
