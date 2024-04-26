using Newtonsoft.Json;

namespace VN_API.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? EnglishName { get; set; }
        public string? Description { get; set; }
        public TagCategory? Category { get; set; }
        public string? VndbId { get; set; }
        public bool? Applicable { get; set; }

        //public int VisualNovel { get; set; } // Visual novel with this tag
    }

    public enum TagCategory
    {
        cont,
        ero,
        tech,
    }
}
