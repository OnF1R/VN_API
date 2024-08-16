using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslatorController : ControllerBase
    {
        private readonly INovelService _novelService;

        public TranslatorController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTranslators()
        {
            var translators = await _novelService.GetTranslatorsAsync();

            if (translators == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, translators);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetTranslator(int id)
        {
            var translator = await _novelService.GetTranslatorAsync(id);

            if (translator == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, translator);
        }

        [HttpPost]
        public async Task<ActionResult<Translator>> AddTranslator([FromQuery] Translator translator)
        {
            var dbTranslator = await _novelService.AddTranslatorAsync(translator);

            if (dbTranslator == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Translator could not be added.");
            }

            return CreatedAtAction("GetTranslator", new { id = dbTranslator.Id }, dbTranslator);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateTranslator([FromQuery] int id, [FromQuery] Translator translator)
        {
            var dbTranslator = await _novelService.UpdateTranslatorAsync(id, translator);

            if (dbTranslator == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Translator could not be updated.");
            }

            if (id != dbTranslator.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteTranslator(int id)
        {
            var translator = await _novelService.GetTranslatorAsync(id);
            (bool status, string message) = await _novelService.DeleteTranslatorAsync(translator);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, translator);
        }
    }
}