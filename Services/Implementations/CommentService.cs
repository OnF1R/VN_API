using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using System.Net;
using VN_API.Database;
using VN_API.Models;
using VN_API.Models.Comment;
using VN_API.Services.Interfaces;

namespace VN_API.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationContext _db;

        public CommentService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<VisualNovelComment> GetCommentAsync(Guid commentId)
        {
            try
            {
                var comment = await _db.Comments
                    .FindAsync(commentId);

                return comment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<VisualNovelComment>> GetCommentReplies(Guid parentCommentId)
        {
            try
            {
                var comments = await _db.Comments
                    .Where(c => c.ParentCommentId == parentCommentId)
                    .ToListAsync();

                return comments;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<VisualNovelComment>> GetUserComments(Guid userId)
        {
            try
            {
                var comments = await _db.Comments
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                return comments;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<VisualNovelCommentWithRating>> GetVisualNovelCommentsAsync(int visualNovelId)
        {
            var commentsQuery = _db.Comments
                .Where(c => c.VisualNovelId == visualNovelId)
                .AsQueryable();

            var commentRatingsQuery = _db.CommentRatings
                .Where(cr => commentsQuery.Select(c => c.Id).Contains(cr.CommentId))
                .AsQueryable();

            List<VisualNovelCommentWithRating> commentWithRating;

            var result = from comment in commentsQuery
                                     join commentRating in commentRatingsQuery
                                     on comment.Id equals commentRating.CommentId into commentRatings
                                     select new VisualNovelCommentWithRating
                                     {
                                         Comment = comment,
                                         Rating = commentRatings
                                         //Rating = commentRatings.Select(cr => new VisualNovelCommentRating
                                         //{
                                         //    CommentId = cr.CommentId,
                                         //    UserId = cr.UserId,
                                         //    IsLike = cr.IsLike
                                         //}),
                                     };


            commentWithRating = await result.ToListAsync();

            return commentWithRating;
        }

        public async Task<VisualNovelComment> UpdateCommentAsync(Guid commentId, VisualNovelComment comment)
        {
            try
            {
                var dbComment = await _db.Comments.Where(c => c.Id == commentId).AsNoTracking().FirstOrDefaultAsync();
                    

                if (dbComment == null)
                {
                    return null;
                }

                comment.IsUpdated = true;

                var sanitizer = new HtmlSanitizer();

                var sanitizedComment = sanitizer.Sanitize(comment.Content);

                comment.Content = sanitizedComment;

                dbComment = comment;

                _db.Comments.Update(dbComment);

                await _db.SaveChangesAsync();

                return dbComment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovelComment> AddCommentAsync(VisualNovelComment comment)
        {
            VisualNovelComment sendingComment;
            try
            {
                var sanitizer = new HtmlSanitizer();

                var sanitizedComment = sanitizer.Sanitize(comment.Content);

                if (comment.ParentCommentId == null)
                {
                    sendingComment = new VisualNovelComment
                    {
                        Content = sanitizedComment,
                        PostedDate = DateTime.Now,
                        VisualNovelId = comment.VisualNovelId,
                        UserId = comment.UserId,

                        ParentCommentId = null,
                    };
                }
                else
                {
                    if (comment.ParentCommentId == null)
                    {
                        return null;
                    }

                    sendingComment = new VisualNovelComment
                    {
                        Content = sanitizedComment,
                        PostedDate = DateTime.Now,
                        VisualNovelId = comment.VisualNovelId,
                        UserId = comment.UserId,

                        ParentCommentId = comment.ParentCommentId,
                    };
                }

                if (sendingComment.ParentCommentId != null)
                {
                    var parentComment = await _db.Comments
                        .Where(c => c.Id == sendingComment.ParentCommentId)
                        .FirstOrDefaultAsync();

                    if (parentComment == null)
                    {
                        return null;
                    }
                }

                _db.Comments.Add(sendingComment);
                
                await _db.SaveChangesAsync();

                return sendingComment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool, string)> DeleteCommentAsync(VisualNovelComment comment)
        {
            try
            {
                var dbComment = await _db.Comments.FindAsync(comment.Id);

                if (dbComment == null)
                {
                    return (false, "Comment could not be found");
                }

                if (await _db.Comments.Where(c => c.ParentCommentId == comment.Id).FirstOrDefaultAsync() != null)
                {
                    //dbComment.Content = "";
                    dbComment.IsDeleted = true;
                    _db.Comments.Entry(dbComment).State = EntityState.Modified;

                    await _db.SaveChangesAsync();
                }
                else
                {
                    _db.Comments.Remove(comment);
                }

                await _db.SaveChangesAsync();

                return (true, "Comment got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }
    }
}
