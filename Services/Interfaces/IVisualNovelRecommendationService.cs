using VN_API.Models;

namespace VN_API.Services.Interfaces
{
    public interface IVisualNovelRecommendationService
    {
        Task<List<VisualNovel>> GetRecommended(int id);
    }
}
