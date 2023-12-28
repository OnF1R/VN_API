using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using VN_API.Models;
using VN_API.Models.Pagination;
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
        public async Task<IActionResult> GetVisualNovels([FromQuery] PaginationParams @params)
        {
            var vns = await _novelService.GetVisualNovelsAsync(@params);

            var paginationMetadata = new PaginationMetadata(vns.Count(), @params.Page, @params.ItemsPerPage);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var paginatedVns = vns.Skip((@params.Page - 1) * @params.ItemsPerPage).Take(@params.ItemsPerPage);

            if (paginatedVns == null || paginatedVns.Count() <= 0)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Visual Novels in database");
            }

            return StatusCode(StatusCodes.Status200OK, paginatedVns);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetVisualNovel(int id)
        {
            VisualNovel vn = await _novelService.GetVisualNovelAsync(id);

            if (vn == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Visual Novel found for id: {id}");
            }

            return StatusCode(StatusCodes.Status200OK, vn);
        }

        [HttpGet("WithTag")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithTag(int tagId)
        {
            var vns = await _novelService.GetVisualNovelsWithTagAsync(tagId);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Visual Novels with this tag in database");
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("WithGenre")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithGenre(int genreId)
        {
            var vns = await _novelService.GetVisualNovelsWithGenreAsync(genreId);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Visual Novels with this genre in database");
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("WithLanguage")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithLanguage(int languageId)
        {
            var vns = await _novelService.GetVisualNovelsWithLanguageAsync(languageId);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Visual Novels with this language in database");
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("WithGamingPlatform")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithGamingPlatform(int gamingPlatformId)
        {
            var vns = await _novelService.GetVisualNovelsWithGamingPlatformAsync(gamingPlatformId);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Visual Novels with this gaming platform in database");
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchVisualNovel(string query)
        {
            var vns = await _novelService.SearchVisualNovel(query);

            if (vns == null || !vns.Any())
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Visual Novels in database with title '{query}'");
            }

            return StatusCode(StatusCodes.Status200OK, vns);
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

        [HttpPut("AddImage")]
        public async Task<IActionResult> AddImage(int id, IFormFile coverImage)
        {
            VisualNovel dbvn = await _novelService.AddCoverImageToVisualNovel(id, coverImage);

            if (dbvn == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Cover image could not be added");
            }

            return CreatedAtAction("GetVisualNovel", new { id = dbvn.Id }, dbvn);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateVisualNovel(int id, VisualNovel visualNovel)
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
        public async Task<IActionResult> DeleteVisualNovel(int id)
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