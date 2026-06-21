using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartBin.Services;
using SmartBin.Models;
using SmartBin.ModelViews;

namespace SmartBin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SurroundingWasteController : ControllerBase
    {
        private readonly ISurroundingWasteService _service;
        private readonly UserManager<AppUser> _userManager;
        public SurroundingWasteController(ISurroundingWasteService service, UserManager<AppUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetLastState(int? BinId, int? CameraId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _service.GetLastStateAsync(BinId, CameraId, user);
            if (result == null)
                return NotFound(new { message = "Device not found or no surrounding waste data available." });
            return Ok(result);

        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory(int? BinId, int? CameraId, int take = 50)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _service.GetHistoryAsync(BinId, CameraId, user, take);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> AddSurroundingWaste([FromForm] SurroundingWasteInsertMV waste, [FromHeader(Name = "Token")] string token)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = _service.AddSurroundingWasteAsync(waste, token);
            if (result == "Bin not found")
                return NotFound(new { message = result });
            return Ok(new { message = result });
        }
    }
}
