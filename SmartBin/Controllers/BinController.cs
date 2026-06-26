using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartBin.Models;
using SmartBin.ModelViews;
using SmartBin.Services;
namespace SmartBin.Controllers
{
    [Route("api/SmartBin")]
    [ApiController]
    public class BinController : ControllerBase
    {
        private readonly IBinService _binService;
        private readonly UserManager<AppUser> _userManager;
        public BinController(IBinService binService, UserManager<AppUser> userManager)
        {
            _binService = binService;
            _userManager = userManager;
        }

        [HttpPost("Create")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<String>> CreateBin()
        {
            string token = await _binService.CreateBin();
            return Ok(token);
        }

        [HttpPost("Assign")]
        [Authorize]
        public async Task<ActionResult> AssignBin([FromBody] AssignBinMV Token)
        {
            AppUser user = await _userManager.GetUserAsync(User);
            string result = await _binService.AssignBin(user, Token);
            if (result != null) { return Ok(result); }
            return BadRequest(result);

        }


        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<BinMV>>> GetAllBins()
        {
            var user = await _userManager.GetUserAsync(User);
            var bins = await _binService.GetAllBins(user);
            return Ok(bins);

        }

        [HttpGet("{Id}")]
        [Authorize]
        public async Task<ActionResult<BinMV>> GetBin([FromRoute]int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var bin = await _binService.GetBin(id, user);
            if (bin == null) return NotFound();
            return Ok(bin);
        }

        [HttpPut("Update")]
        public async Task<ActionResult<BinMV>> UpdateBin(BinUpdateMV bin, [FromHeader(Name = "Token")] string IdentificationToken)
        {
            var updatedBin = await _binService.UpdateBin(bin, IdentificationToken);
            if (updatedBin == null) return NotFound();
            return Ok(updatedBin);
        }

        [HttpPut("{id}/Settings")]
        [Authorize]
        public async Task<ActionResult> UpdateBinSettings([FromRoute] int id, [FromBody] BinSettingsUpdateMV settings)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var success = await _binService.UpdateSettingsAsync(id, settings, user);
            if (!success) return NotFound(new { message = "Bin not found or not owned by user." });

            return Ok(new { message = "Bin settings updated successfully." });
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> DeleteBin([FromHeader(Name = "Token")] string IdentificationToken)
        {
            bool result = await _binService.DeleteBin(IdentificationToken);
            if (result) return Ok("Bin deleted successfully");
            else return NotFound("Bin not found");
        }
    }
}
