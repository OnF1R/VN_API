namespace VN_API.Models.Comment
{
    public class VisualNovelComment
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int VisualNovelId { get; set; }
        public DateTime PostedDate { get; set; }
        public string Content { get; set; }
        public Guid? ParentCommentId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsUpdated { get; set; }
    }
}
