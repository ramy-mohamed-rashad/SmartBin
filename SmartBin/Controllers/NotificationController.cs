using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartBin.Models;
using SmartBin.ModelViews;
using SmartBin.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartBin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        private readonly UserManager<AppUser> _userManager;

        public NotificationController(INotificationService service, UserManager<AppUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationResultMV>>> GetNotifications()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notifications = await _service.GetNotificationsAsync(user);
            return Ok(notifications);
        }

        [HttpPost("{id}/Read")]
        public async Task<IActionResult> MarkAsRead([FromRoute] int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _service.MarkAsReadAsync(id, user);
            if (!success) return NotFound(new { message = "Notification not found or not owned by user." });

            return Ok(new { message = "Notification marked as read." });
        }

        [HttpPost("ReadAll")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            await _service.MarkAllAsReadAsync(user);
            return Ok(new { message = "All notifications marked as read." });
        }
    }
}
