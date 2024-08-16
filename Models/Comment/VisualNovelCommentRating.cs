namespace VN_API.Models.Comment
{
    public class VisualNovelCommentRating
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid CommentId { get; set; }
        public bool IsLike { get; set; }
        public DateTime AddedDate { get; set; }
    }
}
