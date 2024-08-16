using System.ComponentModel.DataAnnotations.Schema;

namespace VN_API.Models
{
    public class VisualNovel
    {
        public int Id { get; set; }
        public string? VndbId { get; set; }
        public string LinkName { get;set; }
        public string Title { get; set; }
        public virtual List<string>? AnotherTitles { get; set; }
        public string? CoverImageLink { get; set; }
        public string? BackgroundImageLink { get; set; }
        public virtual List<string>? ScreenshotLinks{ get; set; }
        public Status Status { get; set; }
        public int PageViewesCount { get; set; }
        public int CommentsCount { get; set; }
        public virtual List<GamingPlatform>? Platforms { get; set; }
        public ReadingTime ReadingTime { get; set; }
        public virtual List<Translator>? Translator { get; set; }
        public virtual List<Author> Author { get; set; }
        public virtual List<Genre>? Genres { get; set; }
        public virtual List<TagMetadata>? Tags { get; set; }
        public virtual List<Language>? Languages { get; set; }
        public ushort? ReleaseYear { get; set; }
        public ushort? ReleaseMonth { get; set; }
        public ushort? ReleaseDay { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateOnly? ReleaseDate { get; private set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }
        public Guid AdddeUserId { get; set; }
        public string AddedUserName { get; set; }
        public string Description { get; set; }
        public virtual List<RelatedNovel>? RelatedNovels { get; set; }
        public virtual List<DownloadLink>? DownloadLinks { get; set; }
        public virtual List<OtherLink>? OtherLinks { get; set; }
        public virtual List<RelatedAnimeLink>? AnimeLinks { get; set; }
        public double? VndbRating { get; set; }
        public int? VndbVoteCount { get; set; }
        public int? VndbLengthInMinutes { get; set; }
        public string? SteamLink { get; set; }
        public string? TranslateLinkForSteam { get; set; }
        public string? SoundtrackYoutubePlaylistLink { get; set; }
    }

    public enum Status
    {
        Release,
        InDevelopment,
        Abandoned,
        Announced
    }

    public enum ReadingTime
    {
        Any,
        LessTwoHours,
        TwoToTenHours,
        TenToThirtyHours,
        ThirtyToFiftyHours,
        OverFiftyHours,
    }

    public enum Sort
    {
        DateUpdatedDescending,
        DateUpdatedAscending,

        ReleaseDateDescending,
        ReleaseDateAscending,

        RatingDescending,
        RatingAscending,

        VoteCountDescending,
        VoteCountAscending,

        VNDBRatingDescending,
        VNDBRatingAscending,

        VNDBVoteCountDescending,
        VNDBVoteCountAscending,
        
        DateAddedDescending,
        DateAddedAscending,

        Title,

    }
}
