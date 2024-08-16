using VN_API.Models.Comment;

namespace VN_API.Services.Interfaces
{
    public interface ICommentRatingService
    {
        Task<VisualNovelCommentRating> GetCommentRatingAsync(Guid ratingId);
        Task<VisualNovelCommentRating> GetCommentRatingAsync(Guid userId, Guid commentId);
        Task<VisualNovelCommentRatingsCount> GetCommentRatingsCount(Guid commentId);
        Task<VisualNovelCommentRating> AddCommentRatingAsync(VisualNovelCommentRating rating);
        Task<VisualNovelCommentRating> UpdateCommentRating(Guid ratingId, VisualNovelCommentRating rating);
        Task<(bool, string)> DeleteCommentRating(VisualNovelCommentRating rating);
    }
}
