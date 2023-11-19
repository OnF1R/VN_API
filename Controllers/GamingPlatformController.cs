using Microsoft.AspNetCore.Mvc;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamingPlatformController : ControllerBase
    {
        private readonly INovelService _novelService;

        public GamingPlatformController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGamingPlatforms()
        {
            var gamingPlatforms = await _novelService.GetGamingPlatformsAsync();

            if (gamingPlatforms == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Gaming Platfroms in database");
            }

            return StatusCode(StatusCodes.Status200OK, gamingPlatforms);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetGamingPlatform(Guid id)
        {
            GamingPlatform gamingPlatform = await _novelService.GetGamingPlatformAsync(id);

            if (gamingPlatform == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Gaming Platform found for id: {id}");
            }

            return StatusCode(StatusCodes.Status200OK, gamingPlatform);
        }

        [HttpPost]
        public async Task<ActionResult<GamingPlatform>> AddGamingPlatform([FromQuery] string gamingPlatformName)
        {
            var dbGamingPlatform = await _novelService.AddGamingPlatformAsync(gamingPlatformName);

            if (dbGamingPlatform == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{gamingPlatformName} could not be added.");
            }

            return CreatedAtAction("GetGamingPlatform", new { id = dbGamingPlatform.Id }, dbGamingPlatform);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateGamingPlatform([FromQuery] Guid id, [FromQuery] string gamingPlatformName)
        {
            GamingPlatform dbGamingPlatform = await _novelService.UpdateGamingPlatformAsync(id, gamingPlatformName);

            if (dbGamingPlatform == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{gamingPlatformName} could not be updated");
            }

            if (id != dbGamingPlatform.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteGamingPlatform(Guid id)
        {
            var gamingPlatform = await _novelService.GetGamingPlatformAsync(id);
            (bool status, string message) = await _novelService.DeleteGamingPlatformAsync(gamingPlatform);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, gamingPlatform);
        }
    }
}