using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using VN_API.Database;
using VN_API.Models.Comment;
using VN_API.Models.Pagination;
using VN_API.Services.Interfaces;

namespace VN_API.Services.Implementations
{
    public class CommentRatingService : ICommentRatingService
    {
        private readonly ApplicationContext _db;

        public CommentRatingService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<VisualNovelCommentRating> GetCommentRatingAsync(Guid ratingId)
        {
            try
            {
                var rating = await _db.CommentRatings.FindAsync(ratingId);

                return rating;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovelCommentRating> GetCommentRatingAsync(Guid userId, Guid commentId)
        {
            try
            {
                var rating = await _db.CommentRatings.Where(cr => cr.UserId == userId && cr.CommentId == commentId).FirstOrDefaultAsync();

                return rating;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovelCommentRatingsCount> GetCommentRatingsCount(Guid commentId)
        {
            try
            {
                var ratings = await _db.CommentRatings.Where(r => r.CommentId == commentId).ToListAsync();
                var likes = ratings.Where(r => r.IsLike).Count();

                var vnCommentRatingCount = new VisualNovelCommentRatingsCount()
                {
                    Likes = likes,
                    Dislikes = ratings.Count - likes,
                };

                return vnCommentRatingCount;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovelCommentRating> AddCommentRatingAsync(VisualNovelCommentRating rating)
        {
            try
            {
                var comment = await _db.Comments.FindAsync(rating.CommentId);

                if (comment == null) 
                {
                    return null;
                }

                var sendingRating = new VisualNovelCommentRating()
                {
                    UserId = rating.UserId,
                    CommentId = rating.CommentId,
                    IsLike = rating.IsLike,
                    AddedDate = DateTime.Now
                };

                _db.CommentRatings.Add(sendingRating);

                await _db.SaveChangesAsync();

                return sendingRating;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<VisualNovelCommentRating> UpdateCommentRating(Guid ratingId, VisualNovelCommentRating rating)
        {
            try
            {
                var dbCommentRating = await _db.CommentRatings.AsNoTracking().Where(r => r.Id == ratingId).FirstOrDefaultAsync();

                if (dbCommentRating == null)
                {
                    return null;    
                }

                dbCommentRating.IsLike = rating.IsLike;

                _db.CommentRatings.Entry(dbCommentRating).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return dbCommentRating;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool, string)> DeleteCommentRating(VisualNovelCommentRating rating)
        {
            try
            {
                var dbCommentRating = await _db.CommentRatings.FindAsync(rating.Id);

                if (dbCommentRating == null)
                {
                    return (false, "Comment rating could not be found");
                }

                _db.CommentRatings.Remove(dbCommentRating);

                await _db.SaveChangesAsync();

                return (true, "Comment rating got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }
    }
}
