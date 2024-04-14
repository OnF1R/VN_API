namespace VN_API.Models
{
    public class Author
    {
        public int Id { get; set; }
        public string? VndbId { get; set; }
        public string Name { get; set; }
        public string? Source { get; set; }
        public virtual List<VisualNovel> VisualNovels { get; set; } = new();
    }
}
