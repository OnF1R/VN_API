using Microsoft.AspNetCore.Mvc;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LanguageController : ControllerBase
    {
        private readonly INovelService _novelService;

        public LanguageController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await _novelService.GetLanguagesAsync();

            if (languages == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, languages);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetLanguage(int id)
        {
            Language language = await _novelService.GetLanguageAsync(id);

            if (language == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, language);
        }

        [HttpPost]
        public async Task<ActionResult<Language>> AddLanguage([FromQuery] string languageName)
        {
            var dbLanguage = await _novelService.AddLanguageAsync(languageName);

            if (dbLanguage == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{languageName} could not be added.");
            }

            return CreatedAtAction("GetLanguage", new { id = dbLanguage.Id }, dbLanguage);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateLanguage([FromQuery] int id, [FromQuery] string languageName)
        {
            Language dbGamingPlatform = await _novelService.UpdateLanguageAsync(id, languageName);

            if (dbGamingPlatform == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{languageName} could not be updated");
            }

            if (id != dbGamingPlatform.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteLanguage(int id)
        {
            var language = await _novelService.GetLanguageAsync(id);
            (bool status, string message) = await _novelService.DeleteLanguageAsync(language);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, language);
        }
    }
}