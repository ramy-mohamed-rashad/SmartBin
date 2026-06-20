using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartBin.Models;
using SmartBin.ModelViews;
using SmartBin.Services;

namespace SmartBin.Controllers
{
    [Route("api/CameraRequest")]
    [ApiController]
    public class CameraRequestController : ControllerBase
    {
        private readonly ICameraRequestService _cameraRequestService;
        private readonly UserManager<AppUser> _userManager;

        public CameraRequestController(ICameraRequestService cameraRequestService, UserManager<AppUser> userManager)
        {
            _cameraRequestService = cameraRequestService;
            _userManager = userManager;
        }

        /// <summary>Normal user submits a camera purchase request.</summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<CameraRequestResultMV>> SubmitRequest([FromBody] CameraRequestInsertMV model)
        {
            if (string.IsNullOrWhiteSpace(model?.RequesterName) || model.Latitude == 0 || model.Longitude == 0)
                return BadRequest(new { Message = "RequesterName, Latitude, and Longitude are required." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _cameraRequestService.SubmitRequest(user.Id, model);
            return Ok(result);
        }

        /// <summary>Admin gets all camera requests.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<CameraRequestResultMV>>> GetAllRequests()
        {
            var requests = await _cameraRequestService.GetAllRequests();
            return Ok(requests);
        }

        /// <summary>Normal user gets their own camera requests.</summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<List<CameraRequestResultMV>>> GetMyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var requests = await _cameraRequestService.GetUserRequests(user.Id);
            return Ok(requests);
        }

        /// <summary>Admin accepts a request → generates camera token.</summary>
        [HttpPost("{id}/Accept")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<CameraRequestResultMV>> AcceptRequest([FromRoute] int id)
        {
            var result = await _cameraRequestService.AcceptRequest(id);
            if (result == null)
                return BadRequest(new { Message = "Request not found or already processed." });

            return Ok(result);
        }
    }
}
