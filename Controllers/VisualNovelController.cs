using Microsoft.AspNetCore.Mvc;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualNovelController : ControllerBase
    {
        private readonly INovelService _novelService;

        public VisualNovelController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetVisualNovels()
        {
            var vns = await _novelService.GetVisualNovelsAsync();

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Visual Novels in database");
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetVisualNovel(Guid id)
        {
            VisualNovel vn = await _novelService.GetVisualNovelAsync(id);

            if (vn == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Visual Novel found for id: {id}");
            }

            return StatusCode(StatusCodes.Status200OK, vn);
        }

        [HttpPost]
        public async Task<ActionResult<VisualNovel>> AddVisualNovel(VisualNovel visualNovel)
        {
            var dbvn = await _novelService.AddVisualNovelAsync(visualNovel);

            if (dbvn == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{visualNovel.Title} could not be added.");
            }

            return CreatedAtAction("GetVisualNovel", new { id = visualNovel.Id }, visualNovel);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateVisualNovel(Guid id, VisualNovel visualNovel)
        {
            if (id != visualNovel.Id)
            {
                return BadRequest();
            }

            VisualNovel dbvn = await _novelService.UpdateVisualNovelAsync(visualNovel);

            if (dbvn == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{visualNovel.Title} could not be updated");
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteVisualNovel(Guid id)
        {
            var vn = await _novelService.GetVisualNovelAsync(id);
            (bool status, string message) = await _novelService.DeleteVisualNovelAsync(vn);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, vn);
        }
    }
}