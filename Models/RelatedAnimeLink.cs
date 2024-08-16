namespace VN_API.Models
{
    public class RelatedAnimeLink
    {
        public Guid Id { get; set; }
        public VisualNovel? VisualNovel { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }
}
