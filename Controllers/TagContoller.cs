using Microsoft.AspNetCore.Mvc;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly INovelService _novelService;

        public TagController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            var tags = await _novelService.GetTagsAsync();

            if (tags == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No Tags in database");
            }

            return StatusCode(StatusCodes.Status200OK, tags);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetTag(Guid id)
        {
            Tag tag = await _novelService.GetTagAsync(id);

            if (tag == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No Tag found for id: {id}");
            }

            return StatusCode(StatusCodes.Status200OK, tag);
        }

        [HttpPost]
        public async Task<ActionResult<Language>> AddTag([FromQuery] string tagName)
        {
            var dbTag = await _novelService.AddTagAsync(tagName);

            if (dbTag == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{tagName} could not be added.");
            }

            return CreatedAtAction("GetTag", new { id = dbTag.Id }, dbTag);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateTag([FromQuery] Guid id, [FromQuery] string tagName)
        {
            Tag dbTag = await _novelService.UpdateTagAsync(id, tagName);

            if (dbTag == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{tagName} could not be updated");
            }

            if (id != dbTag.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteTag(Guid id)
        {
            var tag = await _novelService.GetTagAsync(id);
            (bool status, string message) = await _novelService.DeleteTagAsync(tag);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, tag);
        }
    }
}