namespace VN_API.Models
{
    public class RelatedNovel
    {
        public int VisualNovelId { get; set; }
        public VisualNovel? VisualNovel { get; set; }
        public int RelatedVisualNovelId { get; set; }
        public VisualNovel? RelatedVisualNovel { get; set; }
    }
}
