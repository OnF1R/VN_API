using Microsoft.AspNetCore.Mvc;

namespace VN_API.Models
{
    public class VisualNovel
    {
        public int Id { get; set; }
        public string? VndbId { get; set; }
        public string Title { get; set; }
        public string? OriginalTitle { get; set; }
        public string? CoverImagePath { get; set; } = null;
        public Status Status { get; set; }
        //public int PageViewesCount { get; set; }
        //public int CommentsCount { get; set; }
        public virtual List<GamingPlatform>? Platforms { get; set; }
        public ReadingTime ReadingTime { get; set; }
        public Translator? Translator { get; set; }
        public virtual List<Author> Author { get; set; }
        public virtual List<Genre>? Genres { get; set; }
        public virtual List<TagMetadata>? Tags { get; set; }
        public virtual List<Language> Languages { get; set; }
        public int? ReleaseYear { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateUpdated { get; set; }
        public Guid AdddeUserId { get; set; }
        public string AddedUserName { get; set; }
        public string Description { get; set; }
        public List<DownloadLink>? Links { get; set; }
        public List<OtherLink>? OtherLinks { get; set; }
        public double? VndbRating { get; set; }
        public int? VndbVoteCount { get; set; }
        public int? VndbLengthInMinutes { get; set; }
        public string? SteamLink { get; set; }
        public string? TranslateLinkForSteam { get; set; }
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
