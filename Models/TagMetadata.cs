namespace VN_API.Models
{
    public class TagMetadata
    {
        public Guid Id { get; set; }
        public Tag Tag { get; set; }
        public SpoilerLevel SpoilerLevel { get; set; }
        public VisualNovel? VisualNovel { get; set; }
    }

    public enum SpoilerLevel
    {
        None = 0,
        Minor = 1,
        Major = 2,
    }
}
