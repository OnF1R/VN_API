using VN_API.Models;

namespace VN_API.Services.Interfaces
{
    public interface IVNDBQueriesService
    {
        Task<VNDBQueryResult<VNDBResult>> SearchOnVNDB(string search, string sort = "searchrank");
        Task<VNDBQueryResult<VNDBResult>> GetRating(string id);
        Task<VNDBQueryResult<VNDBResult>> GetRating(List<string> ids);
        Task<VNDBQueryResult<VNDBResult>> GetVisualNovelTags(string id);
        Task<VNDBQueryResult<VNDBTag>> GetTags(string initialId, bool isMore = true);
    }
}
