using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualNovelRatingController : ControllerBase
    {
        private readonly INovelService _novelService;

        public VisualNovelRatingController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRatings()
        {
            var ratings = await _novelService.GetVisualNovelRatingsAsync();

            if (ratings == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Rating in database");
            }

            return StatusCode(StatusCodes.Status200OK, ratings);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetRating(Guid id)
        {
            var rating = await _novelService.GetVisualNovelRatingAsync(id);

            if (rating == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Rating found for id: {id}");
            }

            return StatusCode(StatusCodes.Status200OK, rating);
        }

        [HttpGet("average")]
        public async Task<IActionResult> GetRating(int id)
        {
            var rating = await _novelService.GetVisualNovelAverageRatingWithCount(id);

            if (rating.Item1 == -1 || rating.Item2 == -1)
            {
                return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(rating, Formatting.Indented));
            }

            return StatusCode(StatusCodes.Status200OK, JsonConvert.SerializeObject(rating, Formatting.Indented));
        }

        [HttpPost]
        public async Task<ActionResult<Language>> AddRating([FromQuery] VisualNovelRating vnRating)
        {
            var rating = await _novelService.AddVisualNovelRatingAsync(vnRating);

            if (rating == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Rating could not be added.");
            }

            return CreatedAtAction("GetRating", new { id = rating.Id }, rating);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateRating([FromQuery] Guid id, [FromQuery] int vnRating)
        {
            var rating = await _novelService.UpdateVisualNovelRatingAsync(id, vnRating);

            if (rating == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Rating could not be added.");
            }

            if (id != rating.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteRating(Guid id)
        {
            var rating = await _novelService.GetVisualNovelRatingAsync(id);
            (bool status, string message) = await _novelService.DeleteVisualNovelRatingAsync(rating);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, rating);
        }
    }
}