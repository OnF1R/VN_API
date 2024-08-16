using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using VN_API.Models.Comment;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualNovelCommentRatingController : ControllerBase
    {
        private readonly ICommentRatingService _commentRatingService;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public VisualNovelCommentRatingController(ICommentRatingService commentRatingService, IMemoryCache cache)
        {
            _commentRatingService = commentRatingService ?? throw new ArgumentNullException(nameof(commentRatingService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(10))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));
        }

        [HttpGet]
        public async Task<IActionResult> GetCommentRatingAsync(Guid id)
        {

            var rating = await _commentRatingService.GetCommentRatingAsync(id);


            return StatusCode(StatusCodes.Status200OK, rating);
        }

        [HttpGet("ByUserAndCommentId")]
        public async Task<IActionResult> GetCommentRatingAsync(Guid userId, Guid commentId)
        {
            var rating = await _commentRatingService.GetCommentRatingAsync(userId, commentId);

            return StatusCode(StatusCodes.Status200OK, rating);
        }

        [HttpGet("GetCommentRatingsCount")]
        public async Task<IActionResult> GetCommentRatingsCount(Guid commentId)
        {
            var ratings = await _commentRatingService.GetCommentRatingsCount(commentId);

            if (ratings == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            return StatusCode(StatusCodes.Status200OK, ratings);
        }

        [HttpPost]
        public async Task<IActionResult> AddCommentRating(Guid userId, Guid commentId, bool isLike)
        {
            var comment = new VisualNovelCommentRating()
            {
                UserId = userId,
                CommentId = commentId,
                IsLike = isLike,
            };

            var dbComment = await _commentRatingService.AddCommentRatingAsync(comment);

            if (dbComment == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Comment rating could not be added");
            }

            return StatusCode(StatusCodes.Status200OK, dbComment);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCommentAsync(Guid ratingId, Guid userId, Guid commentId, bool isLike)
        {
            var comment = new VisualNovelCommentRating()
            {
                UserId = userId,
                CommentId = commentId,
                IsLike = isLike,
            };

            var dbRating = await _commentRatingService.UpdateCommentRating(ratingId, comment);

            if (dbRating == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Comment rating could not be updated");
            }

            return StatusCode(StatusCodes.Status200OK, dbRating);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCommentAsync(Guid id)
        {
            var comment = await _commentRatingService.GetCommentRatingAsync(id);
            (bool status, string message) = await _commentRatingService.DeleteCommentRating(comment);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, comment);
        }
    }
}