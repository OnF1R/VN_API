using VN_API.Models.Comment;
using VN_API.Models.Pagination;

namespace VN_API.Services.Interfaces
{
    public interface ICommentService
    {
        Task<VisualNovelComment> GetCommentAsync(Guid commentId);
        Task<List<VisualNovelComment>> GetUserComments(Guid userId);
        Task<List<VisualNovelCommentWithRating>> GetVisualNovelCommentsAsync(int visualNovelId);
        Task<List<VisualNovelComment>> GetCommentReplies(Guid parentCommentId);

        //Task<List<VisualNovelComment>> GetUserComments(Guid userId, PaginationParams @params);
        //Task<List<VisualNovelComment>> GetVisualNovelCommentsAsync(int visualNovelId, PaginationParams @params);
        //Task<List<VisualNovelComment>> GetCommentReplies(Guid parentCommentId, PaginationParams @params);
        
        Task<VisualNovelComment> AddCommentAsync(VisualNovelComment comment);
        Task<VisualNovelComment> UpdateCommentAsync(Guid commentId, VisualNovelComment comment);
        Task<(bool, string)> DeleteCommentAsync(VisualNovelComment comment);
    }
}
