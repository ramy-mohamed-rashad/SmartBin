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
    public class SurroundingWasteController : ControllerBase
    {
        private readonly ISurroundingWasteService _service;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public SurroundingWasteController(
            ISurroundingWasteService service,
            UserManager<AppUser> userManager,
            IWebHostEnvironment env)
        {
            _service     = service;
            _userManager = userManager;
            _env         = env;
        }

        // ── GET: last record for a bin or camera ─────────────────────────────
        /// <summary>Returns the most recent surrounding waste reading for the given device.</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetLastState(int? BinId, int? CameraId)
        {
            var user = await _userManager.GetUserAsync(User);
            var result = await _service.GetLastStateAsync(BinId, CameraId, user!);
            if (result == null)
                return NotFound(new { message = "Device not found or no surrounding waste data available." });
            return Ok(result);
        }

        // ── GET: history ──────────────────────────────────────────────────────
        /// <summary>Returns the last <paramref name="take"/> surrounding waste records for the given device.</summary>
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
        /// Accepts AI-analysed surrounding waste data sent by the external first-hop service.
        /// The camera is identified by the <c>X-Camera-Token</c> request header.
        /// </summary>
        [HttpPost("report")]
        public async Task<IActionResult> ReportSurroundingWaste(
            [FromBody] SurroundingWasteReportMV dto,
            [FromHeader(Name = "X-Camera-Token")] string cameraToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(cameraToken))
                return BadRequest(new { message = "X-Camera-Token header is required." });

            var (success, message) = await _service.ReportSurroundingWasteAsync(dto, cameraToken, _env);
            if (!success)
                return NotFound(new { message });

            return Ok(new { message });
        }
    }
}
