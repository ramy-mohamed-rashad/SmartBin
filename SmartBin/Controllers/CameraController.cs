using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartBin.Models;
using SmartBin.ModelViews;
using SmartBin.Services;

namespace SmartBin.Controllers
{
    [Route("api/Camera")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        private readonly ICameraService _cameraService;
        private readonly UserManager<AppUser> _userManager;

        public CameraController(ICameraService cameraService, UserManager<AppUser> userManager)
        {
            _cameraService = cameraService;
            _userManager = userManager;
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<string>> CreateCamera()
        {
            string token = await _cameraService.CreateCamera();
            return Ok(token);
        }

        [HttpPost("Assign")]
        [Authorize]
        public async Task<ActionResult> AssignCamera([FromBody] AssignBinMV Token) // Reusing AssignBinMV
        {
            AppUser user = await _userManager.GetUserAsync(User);
            string result = await _cameraService.AssignCamera(user, Token);
            if (result != null) { return Ok(result); }
            return BadRequest("Camera not found or already assigned");
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<CameraMV>>> GetAllCameras()
        {
            var user = await _userManager.GetUserAsync(User);
            var cameras = await _cameraService.GetAllCameras(user);
            return Ok(cameras);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<CameraDetailsMV>> GetCamera([FromRoute] int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var camera = await _cameraService.GetCamera(id, user);
            if (camera == null) return NotFound();
            return Ok(camera);
        }

        [HttpPut("Update")]
        public async Task<ActionResult<CameraMV>> UpdateCamera(CameraUpdateMV camera, [FromHeader(Name = "Token")] string IdentificationToken)
        {
            var updatedCamera = await _cameraService.UpdateCamera(camera, IdentificationToken);
            if (updatedCamera == null) return NotFound();
            return Ok(updatedCamera);
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> DeleteCamera([FromHeader(Name = "Token")] string IdentificationToken)
        {
            bool result = await _cameraService.DeleteCamera(IdentificationToken);
            if (result) return Ok("Camera deleted successfully");
            else return NotFound("Camera not found");
        }
    }
}
