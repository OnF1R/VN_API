namespace VN_API.Models
{
    public class VisualNovelRating
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int VisualNovelId { get; set; }
        public int Rating { get; set; }
        public DateTime AddingTime { get; set; }
    }
}
