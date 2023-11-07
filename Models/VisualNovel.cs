namespace VN_API.Models
{
    public class VisualNovel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string? OriginalTitle { get; set; }

        public float Rating { get; set; }

        //public byte[] CoverImage { get; set; }

        //public int PageViewesCount { get; set; }
        //public int CommentsCount { get; set; }

        public List<GamingPlatform> Platforms { get; set; }

        public ReadingTime ReadingTime { get; set; }

        //public Translator? Translator { get; set; }
        public string? Translator { get; set; }
        //public Autor Autor { get; set; }
        public string Autor { get; set; }

        public List<Genre> Genres { get; set; }
        public List<Tag> Tags { get; set; }

        public List<Language> Languages { get; set; }

        public int ReleaseYear { get; set; }

        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }

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
