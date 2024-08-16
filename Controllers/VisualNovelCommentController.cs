using Amazon.S3.Model;
using GTranslatorAPI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Text.Json;
using VN_API.Models.Comment;
using VN_API.Models.Pagination;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualNovelCommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public VisualNovelCommentController(ICommentService commentService, IMemoryCache cache)
        {
            _commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(10))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(60)) 
                ?? throw new ArgumentNullException(nameof(MemoryCacheEntryOptions));
        }

        [HttpGet]
        public async Task<IActionResult> GetComment(Guid id)
        {
            string cacheKey = $"_comment_{id}";

            if (_cache.TryGetValue(cacheKey, out VisualNovelComment comment)) { }
            else
            {
                comment = await _commentService.GetCommentAsync(id);
            }

            _cache.Set(cacheKey, comment, _cacheOptions);

            return StatusCode(StatusCodes.Status200OK, comment);
        }

        [HttpGet("GetVisualNovelComments")]
        public async Task<IActionResult> GetVisualNovelComments(int visualNovelId)
        {
            try
            {
                //string cacheKey = $"_comment_vn_id_{visualNovelId}";

                //if (_cache.TryGetValue(cacheKey, out List<VisualNovelCommentWithRating> comments)) { }
                //else
                //{
                var comments = await _commentService.GetVisualNovelCommentsAsync(visualNovelId);
                //}

                if (comments == null || !comments.Any())
                {
                    return StatusCode(StatusCodes.Status204NoContent);
                }

                //_cache.Set(cacheKey, comments, _cacheOptions);

                return StatusCode(StatusCodes.Status200OK, comments);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet("GetUserComments")]
        public async Task<IActionResult> GetUserComments(Guid userId) 
        {
            string cacheKey = $"_comment_user_id_{userId}";

            if (_cache.TryGetValue(cacheKey, out List<VisualNovelComment> comments)) { }
            else
            {
                comments = await _commentService.GetUserComments(userId);
            }

            if (comments == null || !comments.Any())
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            _cache.Set(cacheKey, comments, _cacheOptions);

            return StatusCode(StatusCodes.Status200OK, comments);
        }

        [HttpGet("GetCommentReplies")]
        public async Task<IActionResult> GetCommentReplies(Guid parentCommentId)
        {
            string cacheKey = $"_comment_parent_comment_id_{parentCommentId}";

            if (_cache.TryGetValue(cacheKey, out List<VisualNovelComment> comments)) { }
            else
            {
                comments = await _commentService.GetCommentReplies(parentCommentId);
            }

            if (comments == null || !comments.Any())
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            _cache.Set(cacheKey, comments, _cacheOptions);

            return StatusCode(StatusCodes.Status200OK, comments);
        }
        
        [HttpPost]
        public async Task<IActionResult> AddComment(Guid userId, int visualNovelId, string content, Guid? parentCommentId = null)
        {
            var comment = new VisualNovelComment()
            {
                UserId = userId,
                VisualNovelId = visualNovelId,
                Content = content,
                ParentCommentId = parentCommentId,
            };

            var dbComment = await _commentService.AddCommentAsync(comment);

            if (dbComment == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Comment could not be added");
            }

            return CreatedAtAction("GetComment", new { id = dbComment.Id }, dbComment);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCommentAsync(Guid commentId, VisualNovelComment comment)
        {
            if (commentId != comment.Id)
            {
                return BadRequest();
            }

            var dbComment = await _commentService.UpdateCommentAsync(commentId, comment);

            if (dbComment == null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Comment could not be updated");
            }

            return StatusCode(StatusCodes.Status200OK, dbComment);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteCommentAsync(Guid commentId)
        {
            var comment = await _commentService.GetCommentAsync(commentId);
            (bool status, string message) = await _commentService.DeleteCommentAsync(comment);

            if (status == false)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, message);
            }

            return StatusCode(StatusCodes.Status200OK, comment);
        }
    }
}