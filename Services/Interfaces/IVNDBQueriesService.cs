using VN_API.Models;

namespace VN_API.Services.Interfaces
{
    public interface IVNDBQueriesService
    {
        Task<VNDBQueryResult> SearchOnVNDB(string search, string sort = "searchrank");
        Task<VNDBQueryResult> GetRating(string id);
        Task<VNDBQueryResult> GetRating(List<string> ids);
    }
}
