using Microsoft.AspNetCore.Mvc;

namespace VN_API.Models
{
    public class VisualNovel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? OriginalTitle { get; set; }
        public byte[]? CoverImage { get; set; } = null;

        //public int PageViewesCount { get; set; }
        //public int CommentsCount { get; set; }

        public virtual List<GamingPlatform> Platforms { get; set; }

        public ReadingTime ReadingTime { get; set; }

        //public Translator? Translator { get; set; }
        public string? Translator { get; set; }
        //public Autor Autor { get; set; }
        public string Autor { get; set; }

        public virtual List<Genre> Genres { get; set; }
        public virtual List<TagMetadata> Tags { get; set; }
        public virtual List<Language> Languages { get; set; }

        public int ReleaseYear { get; set; }

        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

        public Guid AdddeUserId { get; set; }
        public string AddedUserName { get; set; }

        public string Description { get; set; }

        //public DownloadLink[] Links { get; set; }
    }

    public enum ReadingTime
    {
        LessTwoHours,
        TwoToTenHours,
        TenToThirtyHours,
        ThirtyToFiftyHours,
        OverFiftyHours,
    }
}
