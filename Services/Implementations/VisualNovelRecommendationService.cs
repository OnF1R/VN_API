using Amazon.Runtime.Internal.Transform;
using Microsoft.EntityFrameworkCore;
using VN_API.Database;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Services.Implementations
{
    public class VisualNovelRecommendationService : IVisualNovelRecommendationService
    {
        private readonly ApplicationContext _db;

        public VisualNovelRecommendationService(ApplicationContext db)
        {
            _db = db;
        }

        public async Task<List<VisualNovel>> GetRecommended(int id)
        {
            try
            {
                var currentVn = await _db.VisualNovels
                    .Include(vn => vn.Genres)
                    .Include(vn => vn.Tags)
                       .ThenInclude(tag => tag.Tag)
                    .Where(vn => vn.Id == id)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync();

                var vns = _db.VisualNovels
                    .Include(vn => vn.Genres)
                    .Include(vn => vn.Tags)
                       .ThenInclude(tag => tag.Tag)
                    .AsSplitQuery();

                //Todo genres, maybe authors

                if (currentVn.Tags != null || currentVn.Genres != null)
                {
                    var tags = currentVn.Tags.Select(tag => tag.Tag);

                    var genres = currentVn.Genres.Select(genre => genre);

                    var recommended = new Dictionary<VisualNovel, int>();

                    foreach (var vn in vns)
                    {
                        if (vn.Tags != null && vn.Id != id)
                        {
                            var vnTags = vn.Tags.Select(tag => tag.Tag);

                            int weight = vnTags.Intersect(tags).Count();

                            if (vn.Genres != null && currentVn.Genres != null)
                            {
                                var vnGenres = vn.Genres.Select(genre => genre);

                                weight += vnGenres.Intersect(genres).Count() * 10;
                            }

                            recommended.Add(vn, weight);
                        }
                    }

                    recommended = recommended.OrderByDescending(x => x.Value).ToDictionary();

                    foreach (var item in recommended)
                    {
                        Console.WriteLine($"{item.Key.Title} - {item.Value}");
                    }

                    var result = recommended.Keys.Take(20).ToList();

                    foreach (var item in result)
                    {
                        item.Tags?.Clear();
                        item.Genres?.Clear();
                    }

                    return result;
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
