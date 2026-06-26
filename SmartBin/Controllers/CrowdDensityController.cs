using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SmartBin.Services;
using SmartBin.Models;
using SmartBin.ModelViews;

namespace SmartBin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CrowdDensityController : ControllerBase
    {
        private readonly ICrowdDensityService _service;
        private readonly UserManager<AppUser> _userManager;
        public CrowdDensityController(ICrowdDensityService service, UserManager<AppUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLastState(int? BinId, int? CameraId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var result = await _service.GetLastStateAsync(BinId, CameraId, user);
            if (result == null)
                return NotFound(new { message = "Device not found or no crowd density data available." });
            return Ok(result);
        }

        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetHistory(int? BinId, int? CameraId, int take = 50)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var result = await _service.GetHistoryAsync(BinId, CameraId, user, take);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> AddCrowdDensity([FromForm] CrowdDensityInsertMV crowdDensity, [FromHeader(Name = "Token")] string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _service.AddCrowdDensityAsync(crowdDensity, token);
            if (result == "Bin not found")
                return NotFound(new { message = result });
            return Ok(new { message = result });
        }
    }
}
