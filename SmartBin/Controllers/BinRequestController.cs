using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartBin.Models;
using SmartBin.ModelViews;
using SmartBin.Services;

namespace SmartBin.Controllers
{
    [Route("api/BinRequest")]
    [ApiController]
    public class BinRequestController : ControllerBase
    {
        private readonly IBinRequestService _binRequestService;
        private readonly UserManager<AppUser> _userManager;

        public BinRequestController(IBinRequestService binRequestService, UserManager<AppUser> userManager)
        {
            _binRequestService = binRequestService;
            _userManager = userManager;
        }

        /// <summary>Normal user submits a bin purchase request.</summary>
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BinRequestResultMV>> SubmitRequest([FromBody] BinRequestInsertMV model)
        {
            if (string.IsNullOrWhiteSpace(model?.RequesterName) || model.Latitude == 0 || model.Longitude == 0)
                return BadRequest(new { Message = "RequesterName, Latitude, and Longitude are required." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _binRequestService.SubmitRequest(user.Id, model);
            return Ok(result);
        }

        /// <summary>Admin gets all requests.</summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<BinRequestResultMV>>> GetAllRequests()
        {
            var requests = await _binRequestService.GetAllRequests();
            return Ok(requests);
        }

        /// <summary>Normal user gets their own requests.</summary>
        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<List<BinRequestResultMV>>> GetMyRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var requests = await _binRequestService.GetUserRequests(user.Id);
            return Ok(requests);
        }

        /// <summary>Admin accepts a request → generates bin token.</summary>
        [HttpPost("{id}/Accept")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BinRequestResultMV>> AcceptRequest([FromRoute] int id)
        {
            var result = await _binRequestService.AcceptRequest(id);
            if (result == null)
                return BadRequest(new { Message = "Request not found or already processed." });

            return Ok(result);
        }
    }
}
