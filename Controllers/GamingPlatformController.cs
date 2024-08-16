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
        /// <summary>
        /// ��������� ���� ������� ��������
        /// </summary>
        /// <returns></returns>
        [HttpGet("", Name = "��������� ���� ������� ��������")]
        public async Task<IActionResult> GetGamingPlatforms()
        {
            var gamingPlatforms = await _novelService.GetGamingPlatformsAsync();

            if (gamingPlatforms == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, gamingPlatforms);
        }

        /// <summary>
        /// ��������� ���� ������� �������� � ��������� ���������� ������
        /// </summary>
        /// <returns></returns>
        [HttpGet("LoadVisualNovels", Name = "��������� ���� ������� �������� � ��������� ���������� ������")]
        public async Task<IActionResult> GetGamingPlatformsWithVisualNovels()
        {
            var gamingPlatforms = await _novelService.GetGamingPlatformsWithVisualNovelsAsync();

            if (gamingPlatforms == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, gamingPlatforms);
        }
        /// <summary>
        /// ��������� ������� ��������� �� ��������������
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("id", Name = "��������� ������� ��������� �� ��������������")]
        public async Task<IActionResult> GetGamingPlatform(int id)
        {
            GamingPlatform gamingPlatform = await _novelService.GetGamingPlatformAsync(id);

            if (gamingPlatform == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, gamingPlatform);
        }
        /// <summary>
        /// ��������� ������� ��������� �� �������������� � ��������� ���������� ������
        /// </summary>  
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("id/LoadVisualNovels", Name = "��������� ������� ��������� �� �������������� � ��������� ���������� ������")]
        public async Task<IActionResult> GetGamingPlatformWithVisualNovels(int id)
        {
            GamingPlatform gamingPlatform = await _novelService.GetGamingPlatformWithVisualNovelsAsync(id);

            if (gamingPlatform == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
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
        public async Task<IActionResult> UpdateGamingPlatform([FromQuery] int id, [FromQuery] string gamingPlatformName)
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
        public async Task<IActionResult> DeleteGamingPlatform(int id)
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