namespace VN_API.Models
{
    public class Language
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<VisualNovel> VisualNovels { get; set; } = new();
    }
}
