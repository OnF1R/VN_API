using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        private readonly IVNDBQueriesService _VNDBQueriesService;
        private readonly IMemoryCache _cache;

        public VisualNovelController(INovelService novelService, IVNDBQueriesService vNDBQueriesService, IMemoryCache cache)
        {
            _novelService = novelService ?? throw new ArgumentNullException(nameof(novelService));
            _VNDBQueriesService = vNDBQueriesService ?? throw new ArgumentNullException(nameof(vNDBQueriesService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [HttpGet("LoadVNDBRating")]
        public async Task LoadVNDBRating()
        {
            await _novelService.LoadVNDBRating();
        }
        [HttpPut("UpdateVisualNovelLinkName")]
        public async Task UpdateVisualNovelLinkName(int id, string linkName)
        {
            await _novelService.UpdateVisualNovelLinkName(id, linkName);
        }

        [HttpPut("IncrementPageViewsCount")]
        public async Task<IActionResult> IncrementPageViewsCount(int visualNovelId)
        {
            var isSuccess = await _novelService.IncrementPageViewsCount(visualNovelId);

            if (!isSuccess)
                return StatusCode(StatusCodes.Status404NotFound);

            return StatusCode(StatusCodes.Status200OK);
        }

        [HttpGet("LoadOrUpdateVNDBRating")]
        public async Task<IActionResult> LoadVNDBRating(int id)
        {
            try
            {
                await _novelService.LoadOrUpdateVNDBRating(id);

                return StatusCode(StatusCodes.Status200OK, $"For visual novel by id: {id}, update vndb rating");
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status204NoContent);
                //throw;
            }
        }

        [HttpGet("UpdateVisualNovelTagsFromVNDB")]
        public async Task<IActionResult> UpdateVisualNovelTags(int id)
        {
            try
            {
                await _novelService.UpdateVisualNovelTagsFromVNDB(id);
                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status204NoContent);
                //throw;
            }
        }

        [HttpGet("GetRandom")]
        public async Task<IActionResult> GetRandomVisualNovel()
        {
            try
            {
                var vn = await _novelService.GetRandomVisualNovel();

                if (vn == null)
                {
                    return StatusCode(StatusCodes.Status204NoContent);
                }

                return StatusCode(StatusCodes.Status200OK, vn);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status204NoContent);
                //throw;
            }
        }

        //[HttpGet("ParseVNDBTags")]
        //public async Task<IActionResult> ParseVNDBTags()
        //{
        //    try
        //    {
        //        await _novelService.ParseVNDBTags();

        //        return StatusCode(StatusCodes.Status200OK);
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(StatusCodes.Status204NoContent, "Error");
        //    }
        //} Executed

        [HttpGet]
        public async Task<IActionResult> GetVisualNovels([FromQuery] PaginationParams @params)
        {
            var vns = await _novelService.GetVisualNovelsAsync(@params);

            var paginationMetadata = new PaginationMetadata(vns.Count(), @params.Page, @params.ItemsPerPage);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var paginatedVns = vns.Skip((@params.Page - 1) * @params.ItemsPerPage).Take(@params.ItemsPerPage);

            if (paginatedVns == null || paginatedVns.Count() <= 0)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, paginatedVns);
        }

        [HttpGet("withRating")]
        public async Task<IActionResult> GetVisualNovelsWithRating([FromQuery] PaginationParams @params)
        {
            var vns = await _novelService.GetVisualNovelsWithRatingAsync(@params);

            var paginationMetadata = new PaginationMetadata(vns.Count, @params.Page, @params.ItemsPerPage);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var paginatedVns = vns.Skip((@params.Page - 1) * @params.ItemsPerPage).Take(@params.ItemsPerPage);

            if (paginatedVns == null || paginatedVns.Count() <= 0)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, paginatedVns);
        }

        [HttpGet("withRatingFiltred")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetFiltredVisualNovelsWithRatingAsync([FromQuery] PaginationParams @params, 
                [FromQuery] List<int> genres,
                [FromQuery] List<int> tags,
                [FromQuery] List<int> languages,
                [FromQuery] List<int> platforms,
                SpoilerLevel spoilerLevel,
                ReadingTime readingTime,
                Sort sort,
                string search = "")
        {
            string genresString = string.Join("_", genres);
            string tagsString = string.Join("_", tags);
            string languagesString = string.Join("_", languages);
            string platformsString = string.Join("_", platforms);
            string spoilerLevelString = spoilerLevel.ToString();
            string sortString = readingTime.ToString();
            string readingTimeString = sort.ToString();

            string cacheKey = $"_filtred_vn_with_rating_genres_{genresString}_tags_{tagsString}_languages_{languagesString}_" +
                $"platforms_{platformsString}_spoilerLevel_{spoilerLevelString}_sort_{sortString}_readingTime_{readingTimeString}_search_{search}";

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(120))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(900));

            if (_cache.TryGetValue(cacheKey, out List<VisualNovelWithRating> vns)) { }
            else
            {
                vns = await _novelService.GetFiltredVisualNovelsWithRatingAsync
                    (@params, genres, tags, languages, platforms, spoilerLevel, readingTime, sort, search);

            }

            var paginationMetadata = new PaginationMetadata(vns.Count, @params.Page, @params.ItemsPerPage);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var paginatedVns = vns.Skip((@params.Page - 1) * @params.ItemsPerPage).Take(@params.ItemsPerPage);

            if (paginatedVns == null || paginatedVns.Count() <= 0)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            _cache.Set(cacheKey, vns, cacheOptions);

            return StatusCode(StatusCodes.Status200OK, paginatedVns);
        }
        

        [HttpGet("id")]
        public async Task<IActionResult> GetVisualNovel(int id)
        {
            VisualNovel vn = await _novelService.GetVisualNovelAsync(id);

            if (vn == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vn);
        }

        [HttpGet("linkName")]
        public async Task<IActionResult> GetVisualNovel(string linkName)
        {
            VisualNovel vn = await _novelService.GetVisualNovelAsync(linkName);

            if (vn == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vn);
        }

        [HttpGet("GetCoverImage")]
        public async Task<IActionResult> GetVisualNovelCoverImage(int id)
        {
            var img = await _novelService.GetVisualNovelCoverImage(id);

            if (img == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return img;
        }

        [HttpGet("GetBackgroundImage")]
        public async Task<IActionResult> GetVisualNovelBackgroundImage(int id)
        {
            var img = await _novelService.GetVisualNovelBackgroundImage(id);

            if (img == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return img;
        }

        [HttpGet("GetScreenshots")]
        public async Task<IActionResult> GetVisualNovelScreenshots(int id)
        {
            var screenshotsData = await _novelService.GetVisualNovelScreenshotsPath(id);

            if (screenshotsData == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return Ok(screenshotsData);
        }

        [HttpGet("GetImageByPath")]
        public async Task<IActionResult> GetVisualNovelImageByPath(string path)
        {
            var img = await _novelService.GetVisualNovelImageByPath(path);

            if (img == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return img;
        }

        [HttpGet("WithTag")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithTag(int tagId)
        {
            var vns = await _novelService.GetVisualNovelsWithTagAsync(tagId);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("WithTagAndSpoilerLevel")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithTagAndSpoilerLevelAsync(int tagId, SpoilerLevel spoilerLevel)
        {
            var vns = await _novelService.GetVisualNovelsWithTagAndSpoilerLevelAsync(tagId, spoilerLevel);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }



        [HttpGet("WithGenre")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithGenre(int genreId)
        {
            var vns = await _novelService.GetVisualNovelsWithGenreAsync(genreId);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("WithLanguage")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithLanguage(int languageId)
        {
            var vns = await _novelService.GetVisualNovelsWithLanguageAsync(languageId);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("WithGamingPlatform")]
        public async Task<ActionResult<VisualNovel>> GetVisualNovelsWithGamingPlatform(int gamingPlatformId)
        {
            var vns = await _novelService.GetVisualNovelsWithGamingPlatformAsync(gamingPlatformId);

            if (vns == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, vns);
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchVisualNovel(string query)
        {
            var vns = await _novelService.SearchVisualNovel(query);

            if (vns == null || !vns.Any())
            {
                return StatusCode(StatusCodes.Status204NoContent);
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

            return CreatedAtAction("GetVisualNovel", new { id = dbvn.Id }, dbvn);
        }

        [HttpPost("AddVisualNovelFromJson")]
        public async Task<ActionResult<VisualNovel>> AddVisualNovelFromJson(string linkName, IFormFile visualNovelJson, string soundtrackYoutubePlaylistLink = null)
        {
            if (visualNovelJson == null || visualNovelJson.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            using (var reader = new StreamReader(visualNovelJson.OpenReadStream()))
            {
                var jsonString = await reader.ReadToEndAsync();
                VisualNovel visualNovel;

                try
                {
                    visualNovel = JsonSerializer.Deserialize<VisualNovel>(jsonString);

                    visualNovel.LinkName = linkName;

                    visualNovel.SoundtrackYoutubePlaylistLink = soundtrackYoutubePlaylistLink;

                    var dbvn = await _novelService.AddVisualNovelFromJsonAsync(visualNovel);

                    return CreatedAtAction("GetVisualNovel", new { id = dbvn.Id }, dbvn);
                }
                catch (JsonException ex)
                {
                    return BadRequest($"Error deserializing JSON: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }

                // Обработайте объект модели по мере необходимости
            }
        }

        [HttpPut("AddCoverImage")]
        public async Task<IActionResult> AddImage(int id, IFormFile coverImage)
        {
            VisualNovel dbvn = await _novelService.AddCoverImageToVisualNovel(id, coverImage);

            if (dbvn == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Cover image could not be added");
            }

            return CreatedAtAction("GetVisualNovel", new { id = dbvn.Id }, dbvn);
        }

        [HttpPut("AddBackgroundImage")]
        public async Task<IActionResult> AddBackgroundImage(int id, IFormFile backgroundImage)
        {
            VisualNovel dbvn = await _novelService.AddBackgroundImageToVisualNovel(id, backgroundImage);

            if (dbvn == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Background image could not be added");
            }

            return CreatedAtAction("GetVisualNovel", new { id = dbvn.Id }, dbvn);
        }

        [HttpPut("AddScreenshots")]
        public async Task<IActionResult> AddScreenshots(int id, List<IFormFile> screenshots)
        {
            VisualNovel dbvn = await _novelService.AddScreenshotsToVisualNovel(id, screenshots);

            if (dbvn == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Screenshots could not be added");
            }

            return CreatedAtAction("GetVisualNovel", new { id = dbvn.Id }, dbvn);
        }

        [HttpGet("DeleteFromS3")]
        public async Task<IActionResult> DeleteFromS3(string path)
        {
            await _novelService.DeleteFromS3(path);

            return StatusCode(StatusCodes.Status200OK, path);
        }

        [HttpGet("DeleteScreenshotsFolderS3")]
        public async Task<IActionResult> DeleteScreenshotsFolderS3(int vnId)
        {
            await _novelService.DeleteScreenshotsFolderS3(vnId);

            return StatusCode(StatusCodes.Status200OK, vnId);
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

            return CreatedAtAction("GetVisualNovel", new { id = dbvn.Id }, dbvn);
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