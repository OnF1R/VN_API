using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthorController : ControllerBase
    {
        private readonly INovelService _novelService;

        public AuthorController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthors()
        {
            var authors = await _novelService.GetAuthorsAsync();

            if (authors == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, authors);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetAuthor(int id)
        {
            var author = await _novelService.GetAuthorAsync(id);

            if (author == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, author);
        }

        [HttpPost]
        public async Task<ActionResult<Translator>> AddAuthor([FromQuery] Author author)
        {
            var dbAuthor = await _novelService.AddAuthorAsync(author);

            if (dbAuthor == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Author could not be added.");
            }

            return CreatedAtAction("GetAuthor", new { id = dbAuthor.Id }, dbAuthor);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateAuthor([FromQuery] int id, [FromQuery] Author author)
        {
            var dbAuthor = await _novelService.UpdateAuthorAsync(id, author);

            if (dbAuthor == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Author could not be updated.");
            }

            if (id != dbAuthor.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var author = await _novelService.GetAuthorAsync(id);
            (bool status, string message) = await _novelService.DeleteAuthorAsync(author);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, author);
        }
    }
}