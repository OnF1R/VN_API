namespace VN_API.Models
{
    public class Translator
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Source { get; set; }
        public virtual List<VisualNovel> VisualNovels { get; set; } = new();
    }
}
