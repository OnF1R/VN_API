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
                return StatusCode(StatusCodes.Status204NoContent, "No Languages in database");
            }

            return StatusCode(StatusCodes.Status200OK, languages);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetLanguage(Guid id)
        {
            Language language = await _novelService.GetLanguageAsync(id);

            if (language == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Language found for id: {id}");
            }

            return StatusCode(StatusCodes.Status200OK, language);
        }

        [HttpPost]
        public async Task<ActionResult<Language>> AddLanguage(Language language)
        {
            var dbLanguage = await _novelService.AddLanguageAsync(language);

            if (dbLanguage == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{language.Name} could not be added.");
            }

            return CreatedAtAction("GetLanguage", new { id = language.Id }, language);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateLanguage(Guid id, Language language)
        {
            if (id != language.Id)
            {
                return BadRequest();
            }

            Language dbLanguage = await _novelService.UpdateLanguageAsync(language);

            if (dbLanguage == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{language.Name} could not be updated");
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteLanguage(Guid id)
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