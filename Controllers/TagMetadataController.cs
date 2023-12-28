using Microsoft.AspNetCore.Mvc;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagMetadataController : ControllerBase
    {
        private readonly INovelService _novelService;

        public TagMetadataController(INovelService novelService)
        {
            _novelService = novelService;
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetTagMetadata(Guid id)
        {
            var tagMetadata = await _novelService.GetTagMetadata(id);

            if (tagMetadata == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, "No TagMetadata in database");
            }

            return StatusCode(StatusCodes.Status200OK, tagMetadata);
        }

        [HttpGet("tagId")]
        public async Task<IActionResult> GetTagMetadataAsync(int tagId)
        {
            var tagsMetadata = await _novelService.GetTagMetadataAsync(tagId);

            if (tagsMetadata == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No TagsMetadata found for tag id: {tagId}");
            }

            return StatusCode(StatusCodes.Status200OK, tagsMetadata);
        }

        [HttpGet("visualNovelId")]
        public async Task<IActionResult> GetVisualNovelTagsMetadataAsync(int visualNovelId)
        {
            var tagsMetadata = await _novelService.GetTagMetadataAsync(visualNovelId);

            if (tagsMetadata == null)
            {
                return StatusCode(StatusCodes.Status204NoContent, $"No TagsMetadata found for visual novel id: {visualNovelId}");
            }

            return StatusCode(StatusCodes.Status200OK, tagsMetadata);
        }

        [HttpPost]
        public async Task<ActionResult<Language>> AddTagMetadataAsync(int tagId, int visualNovelId, SpoilerLevel spoilerLevel)
        {
            var dbTagMetadata = await _novelService.AddTagMetadataAsync(tagId, visualNovelId, spoilerLevel);

            if (dbTagMetadata == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"TagMetadata could not be added.");
            }

            return CreatedAtAction("GetTagMetadata", new { id = dbTagMetadata.Id }, dbTagMetadata);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateTagMetadataAsync([FromQuery] Guid id, int tagId, int visualNovelId, SpoilerLevel spoilerLevel)
        {
            var dbTagMetadata = await _novelService.UpdateTagMetadataAsync(id, tagId, visualNovelId, spoilerLevel);

            if (dbTagMetadata == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"TagMetadata could not be updated");
            }

            if (id != dbTagMetadata.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        [HttpDelete("id")]
        public async Task<IActionResult> DeleteTagMetadataAsync(Guid id)
        {
            var tag = await _novelService.GetTagMetadata(id);
            (bool status, string message) = await _novelService.DeleteTagMetadataAsync(tag);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, tag);
        }
    }
}