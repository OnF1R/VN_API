using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Net;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VisualNovelRecommendationController : Controller
    {
        private readonly IVisualNovelRecommendationService _recommendationService;
        private readonly IMemoryCache _cache;

        public VisualNovelRecommendationController(IVisualNovelRecommendationService recommendationService, IMemoryCache cache)
        {
            _recommendationService = recommendationService ?? throw new ArgumentNullException(nameof(recommendationService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [HttpGet]
        public async Task<IActionResult> GetRecommendations(int id)
        {
            string cacheKey = $"recommendations_vnId_{id}";

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(120))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(900));

            if (_cache.TryGetValue(cacheKey, out List<VisualNovel> recommendations)) { }
            else
            {
                recommendations = await _recommendationService.GetRecommended(id);
            }

            if (recommendations == null)
            {
                return StatusCode(StatusCodes.Status204NoContent);
            }

            _cache.Set(cacheKey, recommendations, cacheOptions);

            return StatusCode(StatusCodes.Status200OK, recommendations);
        }
    }
}
