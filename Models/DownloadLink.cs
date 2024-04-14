namespace VN_API.Models
{
    public class DownloadLink
    {
        public Guid Id { get; set; }
        public VisualNovel VisualNovel { get; set; }
        public GamingPlatform GamingPlatform { get; set; }
        public string Url { get; set; }
    }
}
