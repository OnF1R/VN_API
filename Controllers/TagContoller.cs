using GTranslatorAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Net;
using System.Text.Json;
using VN_API.Models;
using VN_API.Models.Pagination;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly INovelService _novelService;
        private readonly IMemoryCache _cache;

        public TagController(INovelService novelService, IMemoryCache cache)
        {
            _novelService = novelService ?? throw new ArgumentNullException(nameof(novelService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [HttpGet]
        public async Task<IActionResult> GetTags([FromQuery] PaginationParams @params)
        {
            var tags = await _novelService.GetTagsAsync(@params);

            var paginationMetadata = new PaginationMetadata(tags.Item2, @params.Page, @params.ItemsPerPage);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            if (tags.Item1 == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, tags.Item1);
        }

        [HttpGet("All")]
        public async Task<IActionResult> GetAllTags()
        {
            string cacheKey = "allTags";

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(30))
                .SetAbsoluteExpiration(TimeSpan.FromDays(90)); //Tags are so rarely changed

            if (_cache.TryGetValue(cacheKey, out List<Tag> tags)) { }
            else
            {
                tags = await _novelService.GetAllTagsAsync();
            }

            if (tags == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            _cache.Set(cacheKey, tags, cacheOptions);

            return StatusCode(StatusCodes.Status200OK, tags);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchTags([FromQuery] PaginationParams @params, string query)
        {
            var tags = await _novelService.SearchTags(@params, query);

            var paginationMetadata = new PaginationMetadata(tags.Item2, @params.Page, @params.ItemsPerPage);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            if (tags.Item1 == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, tags.Item1);
        }

        [HttpGet("id")]
        public async Task<IActionResult> GetTag(int id)
        {
            Tag tag = await _novelService.GetTagAsync(id);

            if (tag == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, tag);
        }

        [HttpPost]
        public async Task<ActionResult<Language>> AddTag([FromQuery] string tagName, [FromQuery] string description)
        {
            var dbTag = await _novelService.AddTagAsync(tagName, description);

            if (dbTag == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"{tagName} could not be added.");
            }

            return CreatedAtAction("GetTag", new { id = dbTag.Id }, dbTag);
        }

        [HttpPut("id")]
        public async Task<IActionResult> UpdateTag([FromQuery] int id, [FromQuery] string tagName)
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
        public async Task<IActionResult> DeleteTag(int id)
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