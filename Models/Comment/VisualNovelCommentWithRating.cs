namespace VN_API.Models.Comment
{
    public class VisualNovelCommentWithRating
    {
        public VisualNovelComment Comment { get; set; }
        public IEnumerable<VisualNovelCommentRating> Rating { get; set; }
    }
}
