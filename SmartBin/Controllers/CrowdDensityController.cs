using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IWebHostEnvironment _env;

        public CrowdDensityController(
            ICrowdDensityService service,
            UserManager<AppUser> userManager,
            IWebHostEnvironment env)
        {
            _service     = service;
            _userManager = userManager;
            _env         = env;
        }

        // ── GET: last record for a bin or camera ─────────────────────────────
        /// <summary>Returns the most recent crowd density reading for the given device.</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLastState(int? BinId, int? CameraId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _service.GetLastStateAsync(BinId, CameraId, user!);
            if (result == null)
                return NotFound(new { message = "Device not found or no crowd density data available." });
            return Ok(result);
        }

        // ── GET: history ──────────────────────────────────────────────────────
        /// <summary>Returns the last <paramref name="take"/> crowd density records for the given device.</summary>
        [HttpGet("history")]
        [Authorize]
        public async Task<IActionResult> GetHistory(int? BinId, int? CameraId, int take = 50)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _service.GetHistoryAsync(BinId, CameraId, user!, take);
            return Ok(result);
        }

        // ── POST: receive AI result from external first-hop service ───────────
        /// <summary>
        /// Accepts AI-analysed crowd density data sent by the external first-hop service.
        /// <list type="bullet">
        ///   <item><c>X-Camera-Token</c> — identifies the camera that captured the image.</item>
        ///   <item><c>X-Bin-Token</c>    — identifies the nearby bin this crowd reading is linked to.</item>
        /// </list>
        /// </summary>
        [HttpPost("report")]
        public async Task<IActionResult> ReportCrowdDensity(
            [FromBody] CrowdDensityReportMV dto,
            [FromHeader(Name = "X-Camera-Token")] string cameraToken,
            [FromHeader(Name = "X-Bin-Token")] string binToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(cameraToken))
                return BadRequest(new { message = "X-Camera-Token header is required." });

            if (string.IsNullOrWhiteSpace(binToken))
                return BadRequest(new { message = "X-Bin-Token header is required." });

            var (success, message) = await _service.ReportCrowdDensityAsync(dto, cameraToken, binToken, _env);
            if (!success)
                return NotFound(new { message });

            return Ok(new { message });
        }
    }
}
