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
        public async Task<ActionResult<GamingPlatform>> AddGamingPlatform(GamingPlatform gamingPlatform)
        {
            var dbGamingPlatform = await _novelService.AddGamingPlatformAsync(gamingPlatform);

            if (dbGamingPlatform == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{gamingPlatform.Name} could not be added.");
            }

            return CreatedAtAction("GetGamingPlatform", new { id = gamingPlatform.Id }, gamingPlatform);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateGamingPlatform(Guid id, GamingPlatform gamingPlatform)
        {
            if (id != gamingPlatform.Id)
            {
                return BadRequest();
            }

            GamingPlatform dbGamingPlatform = await _novelService.UpdateGamingPlatformAsync(gamingPlatform);

            if (dbGamingPlatform == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{gamingPlatform.Name} could not be updated");
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