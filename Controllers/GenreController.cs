using Microsoft.AspNetCore.Mvc;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GenreController : ControllerBase
    {
        private readonly INovelService _novelService;

        public GenreController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            var genres = await _novelService.GetGenresAsync();

            if (genres == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Genres in database");
            }

            return StatusCode(StatusCodes.Status200OK, genres);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetGenre(Guid id)
        {
            Genre genre = await _novelService.GetGenreAsync(id);

            if (genre == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Genre found for id: {id}");
            }

            return StatusCode(StatusCodes.Status200OK, genre);
        }

        [HttpPost]
        public async Task<ActionResult<Genre>> AddGenre(Genre genre)
        {
            var dbGenre = await _novelService.AddGenreAsync(genre);

            if (dbGenre == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{genre.Name} could not be added.");
            }

            return CreatedAtAction("GetGenre", new { id = genre.Id }, genre);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateGenre(Guid id, Genre genre)
        {
            if (id != genre.Id)
            {
                return BadRequest();
            }

            Genre dbGenre = await _novelService.UpdateGenreAsync(genre);

            if (dbGenre == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{genre.Name} could not be updated");
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteGenre(Guid id)
        {
            var genre = await _novelService.GetGenreAsync(id);
            (bool status, string message) = await _novelService.DeleteGenreAsync(genre);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, genre);
        }
    }
}