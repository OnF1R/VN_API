namespace VN_API.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public virtual List<VisualNovel> VisualNovels { get; set; } = new();
    }
}
