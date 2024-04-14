using Newtonsoft.Json;

namespace VN_API.Models
{
    public enum DevelopmentStatus
    {
        Finished = 0,
        InDevelopment = 1,
        Canceled = 2
    }

    public enum DeveloperType
    {
        Company,
        Individual,
        AmateurGroup,
        Undefined
    }

    public class VNDBDeveloper
    {
        public VNDBDeveloper()
        {
            DeveloperType = Type switch
            {
                "co" => DeveloperType.Company,
                "in" => DeveloperType.Individual,
                "ng" => DeveloperType.AmateurGroup,
                _ => DeveloperType.Undefined,
            };
        }

        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Original { get; set; }
        public List<string>? Aliases { get; set; }
        [JsonProperty("lang")]
        public string? Language { get; set; }
        public string? Type { get; set; }
        public DeveloperType DeveloperType { get; set; }
        public string? Description { get; set; }
    }

    public class VNDBTitle
    {
        [JsonProperty("lang")]
        public string? Language { get; set; }
        public string? Title { get; set; }
        public string? Latin { get; set; }
        [JsonProperty("official")]
        public bool? IsOfficial { get; set; }
        [JsonProperty("main")]
        public bool? IsMain { get; set; }
    }

    public class VNDBImage
    {
        public VNDBImage()
        {
            if (Dims != null)
            {
                Width = Dims[0];
                Height = Dims[1];
            }
        }

        public string? Id { get; set; }
        public string? Url { get; set; }
        public int[]? Dims { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public short? Sexual { get; set; }
        public short? Violence { get; set; }
        public int? VoteCount { get; set; }
    }

    public class VNDBScreenshot : VNDBImage
    {
        public VNDBScreenshot() : base()
        {
            if (ThumbnailDims != null)
            {
                ThumbnailWidth = ThumbnailDims[0];
                ThumbnailHeight = ThumbnailDims[1];
            }
        }

        public string? Thumbnail { get; set; }
        [JsonProperty("thumbnail_dims")]
        public int[]? ThumbnailDims { get; set; }
        public int? ThumbnailWidth { get; set; }
        public int? ThumbnailHeight { get; set; }
    }

    public class VNDBQueryResult
    {
        public bool More { get; set; }
        public List<VNDBResult>? Results { get; set; }
    }
    public class VNDBResult
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? AltTitle { get; set; }
        public List<VNDBTitle>? Titles { get; set; }
        public List<string>? Aliases { get; set; }
        public string? Olang { get; set; }
        public DevelopmentStatus? Devstatus { get; set; }
        public DateOnly? Released { get; set; }
        public List<VNDBImage>? Image { get; set; }
        public int? Length { get; set; }
        [JsonProperty("length_minutes")]
        public int? LengthInMinutes { get; set; }
        [JsonProperty("length_votes")]
        public int? LengthVotes { get; set; }
        public string? Description { get; set; }
        public double? Rating { get; set; }
        public int? VoteCount { get; set; }
        public List<VNDBScreenshot>? Screenshots { get; set; }
        public List<VNDBDeveloper>? Developers { get; set; }

        //Todo: Maybe add other https://api.vndb.org/kana#query-format end on relations
    }
}
