using FuzzySharp;
using Ganss.Xss;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Linq;
using VN_API.Database;
using VN_API.Models;
using VN_API.Models.Pagination;
using VN_API.Services.Implimentations;
using VN_API.Services.Interfaces;

namespace VN_API.Services
{
    public class NovelAdderService : INovelService
    {
        private const string VisualNovelListCacheKey = "VisualNovelList";
        private readonly ApplicationContext _db;
        private IMemoryCache _cache;
        private Amazon.S3.IAmazonS3 _s3Client;
        private const string bucketName = "astral-novel";
        private const string _s3Url = "https://2f58d602-2c33-481e-875b-700b4d4b3263.selstorage.ru/";

        public NovelAdderService(ApplicationContext db, IMemoryCache cache, Amazon.S3.IAmazonS3 s3Client)
        {
            _db = db;
            _cache = cache;
            _s3Client = s3Client;
        }

        public async Task<bool> IncrementPageViewsCount(int visualNovelId)
        {
            try
            {
                var vn = await _db.VisualNovels.FindAsync(visualNovelId);

                if (vn == null)
                {
                    return false;
                }

                vn.PageViewesCount++;

                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public async Task<VisualNovel> GetRandomVisualNovel()
        {
            try
            {
                int count = await _db.VisualNovels.CountAsync();
                int index = new Random().Next(count);

                var novel = await _db.VisualNovels.Skip(index).FirstOrDefaultAsync();

                if (novel == null)
                {
                    return null;
                }

                return novel;
            }
            catch (Exception)
            {
                return null;
                throw;
            }
        }

        public async Task LoadVNDBRating()
        {
            try
            {
                var vndbService = new VNDBQueriesService();

                var visualNovels = await _db.VisualNovels.Where(vn => vn.VndbId != null).ToListAsync();

                List<string> vnWithVndbId = visualNovels.Select(vn => vn.VndbId).ToList();

                var vndbResult = await vndbService.GetRating(vnWithVndbId);

                if (vndbResult != null && vndbResult.Results != null)
                {
                    foreach (var vndb in vndbResult.Results)
                    {
                        var vn = visualNovels.Where(vn => vn.VndbId == vndb.Id).First();
                        if (vndb.Rating != null)
                            vn.VndbRating = Math.Round((double)vndb.Rating / 10, 2);
                        if (vndb.VoteCount != null)
                            vn.VndbVoteCount = vndb.VoteCount;
                        if (vndb.LengthInMinutes != null)
                            vn.VndbLengthInMinutes = vndb.LengthInMinutes;

                        _db.Entry(vn).State = EntityState.Modified;
                    }
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task LoadOrUpdateVNDBRating(int id)
        {
            try
            {
                var vndbService = new VNDBQueriesService();

                VisualNovel visualNovel = await _db.VisualNovels.FirstAsync(vn => vn.Id == id);

                if (visualNovel.VndbId != null)
                {
                    var vndbResult = await vndbService.GetRating(visualNovel.VndbId.ToString());

                    if (vndbResult != null && vndbResult.Results != null)
                    {
                        if (vndbResult.Results[0].Rating != null)
                            visualNovel.VndbRating = Math.Round((double)vndbResult.Results[0].Rating / 10, 2);
                        if (vndbResult.Results[0] != null)
                            visualNovel.VndbVoteCount = vndbResult.Results[0].VoteCount;
                        if (vndbResult.Results[0].LengthInMinutes != null)
                            visualNovel.VndbLengthInMinutes = vndbResult.Results[0].LengthInMinutes;

                        _db.Entry(visualNovel).State = EntityState.Modified;
                    }

                    await _db.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public static string FirstCharToUpper(string input) =>
        input switch
        {
            null => throw new ArgumentNullException(nameof(input)),
            "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
            _ => input[0].ToString().ToUpper() + input.Substring(1)
        };

        public async Task ParseVNDBTags()
        {
            var vndbService = new VNDBQueriesService();
            var translator = new GTranslatorAPI.Translator();
            bool isMore = false;
            string currentTagId = "";

            //TODO Not Add The Same

            do
            {
                var tagsData = await vndbService.GetTags(currentTagId, isMore);
                isMore = tagsData.More;

                foreach (var tag in tagsData.Results)
                {
                    var translateResult = await translator.TranslateAsync(GTranslatorAPI.Languages.en, GTranslatorAPI.Languages.ru, tag.Name);
                    var tranlatedName = FirstCharToUpper(translateResult.TranslatedText);

                    var newTag = new Tag
                    {
                        VndbId = tag.Id,
                        EnglishName = tag.Name,
                        Name = tranlatedName,
                        Category = tag.Category,
                        Applicable = tag.Applicable,
                    };

                    await _db.Tags.AddAsync(newTag);

                    Console.WriteLine($"{tranlatedName} тег добавлен.");
                }

                currentTagId = tagsData.Results[^1].Id;

            } while (isMore);

            await _db.SaveChangesAsync();
        }

        public async Task UpdateVisualNovelTagsFromVNDB(int id)
        {
            var vndbService = new VNDBQueriesService();

            VisualNovel visualNovel = await _db.VisualNovels.Include(vn => vn.Tags).FirstAsync(vn => vn.Id == id);

            if (visualNovel.VndbId != null)
            {
                var vndbResult = await vndbService.GetVisualNovelTags(visualNovel.VndbId.ToString());

                if (visualNovel.Tags != null)
                    visualNovel.Tags.Clear();

                if (vndbResult != null && vndbResult.Results != null)
                {
                    foreach (var tag in vndbResult.Results[0].Tags)
                    {
                        var tagMetadata = new TagMetadata
                        {
                            Id = Guid.NewGuid(),
                            Tag = await _db.Tags.Where(t => t.VndbId == tag.Id).FirstOrDefaultAsync(),
                            SpoilerLevel = (SpoilerLevel)tag.Spoiler,
                            VisualNovel = visualNovel,
                        };

                        if (tagMetadata.Tag == null)
                            continue;

                        await _db.TagsMetadata.AddAsync(tagMetadata);
                    }
                }

                await _db.SaveChangesAsync();
            }
        }

        #region Visual Novel

        public async Task<List<VisualNovel>> GetVisualNovelsAsync(PaginationParams @params)
        {
            try
            {
                //cache.TryGetValue(id, out User? user);

                //var visualNovels = _db.VisualNovels
                //    .Include(vn => vn.Genres)
                //    .Include(vn => vn.Tags)
                //    .Include(vn => vn.Platforms)
                //    .Include(vn => vn.Languages)
                //    .ToListAsync();
                // Without Lazy Loading The Same Result

                var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(90));

                if (_cache.TryGetValue(VisualNovelListCacheKey, out List<VisualNovel> visualNovels))
                    return visualNovels;

                visualNovels = await _db.VisualNovels.ToListAsync();

                _cache.Set(VisualNovelListCacheKey + $"_page_{@params.Page}", visualNovels, cacheOptions);

                return visualNovels;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<VisualNovelWithRating>> GetVisualNovelsWithRatingAsync(PaginationParams @params)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600));

                if (_cache.TryGetValue(VisualNovelListCacheKey + $"_page_with_rating_{@params.Page}", out List<VisualNovelWithRating> visualNovels))
                    return visualNovels;

                var query = from novel in _db.VisualNovels
                            join rating in _db.Rating
                                on novel.Id equals rating.VisualNovelId into novelRatings
                            select new VisualNovelWithRating
                            {
                                VisualNovel = novel,
                                AverageRating = novelRatings.Where(r => r.VisualNovelId == novel.Id).Any() ? novelRatings.Where(r => r.VisualNovelId == novel.Id).Average(r => r.Rating) : 0,
                                NumberOfRatings = novelRatings.Where(r => r.VisualNovelId == novel.Id).Count()
                            };

                visualNovels = await query.ToListAsync();

                _cache.Set(VisualNovelListCacheKey + $"_page_with_rating_{@params.Page}", visualNovels, cacheOptions);

                return visualNovels;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<VisualNovelWithRating>> GetFiltredVisualNovelsWithRatingAsync
            (
                PaginationParams @params,
                List<int> genres,
                List<int> tags,
                List<int> languages,
                List<int> platforms,
                SpoilerLevel spoilerLevel,
                ReadingTime readingTime,
                Sort sort,
                string search
            )
        {
            try
            {
                List<VisualNovelWithRating> visualNovels;

                var vns = _db.VisualNovels.AsQueryable().AsNoTracking();

                if (readingTime != ReadingTime.Any)
                    vns = vns.Where(vn => vn.ReadingTime == readingTime);

                #region Include And Exclude Data
                var includedGenres = genres.Where(id => id > 0).Select(Math.Abs).ToList();
                var excludedGenres = genres.Where(id => id < 0).Select(Math.Abs).ToList();
                var includedTags = tags.Where(id => id > 0).Select(Math.Abs).ToList();
                var excludedTags = tags.Where(id => id < 0).Select(Math.Abs).ToList();
                var includedLanguages = languages.Where(id => id > 0).Select(Math.Abs).ToList();
                var excludedLanguages = languages.Where(id => id < 0).Select(Math.Abs).ToList();
                var includedPlatforms = platforms.Where(id => id > 0).Select(Math.Abs).ToList();
                var excludedPlatforms = platforms.Where(id => id < 0).Select(Math.Abs).ToList();

                foreach (var genreId in includedGenres)
                {
                    var currentGenreId = genreId;
                    vns = vns.Where(vn => vn.Genres.Any(g => g.Id == currentGenreId));
                }

                if (excludedGenres.Any())
                {
                    vns = vns.Where(vn => !vn.Genres.Any(g => excludedGenres.Contains(g.Id)));
                }

                foreach (var tagId in includedTags)
                {
                    var currentTagId = tagId;
                    vns = vns.Where(vn => vn.Tags.Any(t => t.Tag.Id == currentTagId && t.SpoilerLevel <= spoilerLevel));
                }

                if (excludedTags.Any())
                {
                    vns = vns.Where(vn => !vn.Tags.Any(t => excludedTags.Contains(t.Tag.Id)));
                }

                foreach (var languageId in includedLanguages)
                {
                    var currentLanguageId = languageId;
                    vns = vns.Where(vn => vn.Languages.Any(l => l.Id == currentLanguageId));
                }

                if (excludedLanguages.Any())
                {
                    vns = vns.Where(vn => !vn.Languages.Any(l => excludedLanguages.Contains(l.Id)));
                }

                foreach (var platformId in includedPlatforms)
                {
                    var currentPlatformId = platformId;
                    vns = vns.Where(vn => vn.Platforms.Any(p => p.Id == currentPlatformId));
                }

                if (excludedPlatforms.Any())
                {
                    vns = vns.Where(vn => !vn.Platforms.Any(p => excludedPlatforms.Contains(p.Id)));
                }

                //if (!string.IsNullOrEmpty(search))
                //{
                //    int matchPercent = 80;

                //    string searchLowerCase = search.ToLower();

                //    var temp1 = vns
                //        .Where(vn => Fuzz.Ratio(vn.Title.ToLower(), searchLowerCase) >= matchPercent);
                //        //.Where(vn => vn.Title.ToLower().Contains(searchLowerCase));

                //    var temp2 = vns.Where(vn => vn.AnotherTitles.Any(t => Fuzz.Ratio(t.ToLower(), searchLowerCase) >= matchPercent));
                //    //var temp2 = vns.Where(vn => vn.AnotherTitles.Any(t => t.ToLower().Contains(searchLowerCase)));

                //    vns = temp1.Concat(temp2);

                //    vns = vns.Distinct();
                //}
                #endregion

                var query = from novel in vns
                            join rating in _db.Rating
                                on novel.Id equals rating.VisualNovelId into novelRatings
                            select new VisualNovelWithRating
                            {
                                VisualNovel = novel,
                                AverageRating = novelRatings.Where(r => r.VisualNovelId == novel.Id).Any() ? novelRatings.Where(r => r.VisualNovelId == novel.Id).Average(r => r.Rating) : 0,
                                NumberOfRatings = novelRatings.Where(r => r.VisualNovelId == novel.Id).Count()
                            };

                visualNovels = await query.ToListAsync();

                if (!string.IsNullOrEmpty(search))
                {
                    int mainMatchPercent = 50;
                    int anotherMatchPercent = 76;

                    string searchLowerCase = search.ToLower();

                    var temp1 = visualNovels
                        .Where(vn => Fuzz.Ratio(vn.VisualNovel.Title.ToLower(), searchLowerCase) >= mainMatchPercent)
                        .ToList();

                    if (temp1.Count == 0)
                    {
                        temp1 = visualNovels
                        .Where(vn => Fuzz.WeightedRatio(vn.VisualNovel.Title.ToLower(), searchLowerCase) >= mainMatchPercent)
                        .ToList();
                    }

                    var temp2 = visualNovels
                        .Where(vn => vn.VisualNovel.AnotherTitles != null && vn.VisualNovel.AnotherTitles.Any(t => Fuzz.WeightedRatio(t.ToLower(), searchLowerCase) >= anotherMatchPercent))
                        .ToList();

                    visualNovels = temp1.Concat(temp2).ToList();

                    visualNovels = visualNovels.Distinct().ToList();
                }

                switch (sort)
                {
                    case Sort.DateUpdatedDescending:
                        visualNovels = visualNovels.OrderByDescending(vn => vn.VisualNovel.DateUpdated).ToList();
                        break;
                    case Sort.DateUpdatedAscending:
                        visualNovels = visualNovels.OrderBy(vn => vn.VisualNovel.DateUpdated).ToList();
                        break;
                    case Sort.ReleaseDateDescending:
                        //visualNovels = visualNovels.OrderByDescending(vn => vn.VisualNovel.ReleaseDate).ToList(); TODO
                        visualNovels = visualNovels
                            .OrderByDescending(vn =>
                            new DateTime(
                                vn.VisualNovel.ReleaseYear != null ? (int)vn.VisualNovel.ReleaseYear : 1900,
                                vn.VisualNovel.ReleaseMonth != null ? (int)vn.VisualNovel.ReleaseMonth : 1,
                                vn.VisualNovel.ReleaseDay != null ? (int)vn.VisualNovel.ReleaseDay : 1))
                            .ToList();
                        break;
                    case Sort.ReleaseDateAscending:
                        //visualNovels = visualNovels.OrderBy(vn => vn.VisualNovel.ReleaseDate).ToList(); TODO
                        visualNovels = visualNovels
                            .OrderBy(vn => 
                            new DateTime(
                                vn.VisualNovel.ReleaseYear != null ? (int)vn.VisualNovel.ReleaseYear : 1900,
                                vn.VisualNovel.ReleaseMonth != null ? (int)vn.VisualNovel.ReleaseMonth : 1,
                                vn.VisualNovel.ReleaseDay != null ? (int)vn.VisualNovel.ReleaseDay : 1))
                            .ToList();
                        break;
                    case Sort.RatingDescending:
                        visualNovels = visualNovels.OrderByDescending(vn => vn.AverageRating).ToList();
                        break;
                    case Sort.RatingAscending:
                        visualNovels = visualNovels.OrderBy(vn => vn.AverageRating).ToList();
                        break;
                    case Sort.VoteCountDescending:
                        visualNovels = visualNovels.OrderByDescending(vn => vn.NumberOfRatings).ToList();
                        break;
                    case Sort.VoteCountAscending:
                        visualNovels = visualNovels.OrderBy(vn => vn.NumberOfRatings).ToList();
                        break;
                    case Sort.VNDBRatingDescending:
                        var withoutVndbRating = visualNovels.Where(vn => vn.VisualNovel.VndbRating == null);
                        visualNovels = visualNovels.Where(vn => vn.VisualNovel.VndbRating != null).OrderByDescending(vn => vn.VisualNovel.VndbRating).ToList();
                        visualNovels = visualNovels.Concat(withoutVndbRating).ToList();
                        break;
                    case Sort.VNDBRatingAscending:
                        var withoutVndbRating_2 = visualNovels.Where(vn => vn.VisualNovel.VndbRating == null);
                        visualNovels = visualNovels.Where(vn => vn.VisualNovel.VndbRating != null).OrderBy(vn => vn.VisualNovel.VndbRating).ToList();
                        visualNovels = withoutVndbRating_2.Concat(visualNovels).ToList();
                        break;
                    case Sort.VNDBVoteCountDescending:
                        var withoutVndbVoteCount = visualNovels.Where(vn => vn.VisualNovel.VndbVoteCount == null);
                        visualNovels = visualNovels.Where(vn => vn.VisualNovel.VndbVoteCount != null).OrderByDescending(vn => vn.VisualNovel.VndbVoteCount).ToList();
                        visualNovels = visualNovels.Concat(withoutVndbVoteCount).ToList();
                        break;
                    case Sort.VNDBVoteCountAscending:
                        var withoutVndbVoteCount_2 = visualNovels.Where(vn => vn.VisualNovel.VndbVoteCount == null);
                        visualNovels = visualNovels.Where(vn => vn.VisualNovel.VndbVoteCount != null).OrderBy(vn => vn.VisualNovel.VndbVoteCount).ToList();
                        visualNovels = withoutVndbVoteCount_2.Concat(visualNovels).ToList();
                        break;
                    case Sort.DateAddedDescending:
                        visualNovels = visualNovels.OrderByDescending(vn => vn.VisualNovel.DateAdded).ToList();
                        break;
                    case Sort.DateAddedAscending:
                        visualNovels = visualNovels.OrderBy(vn => vn.VisualNovel.DateAdded).ToList();
                        break;
                    case Sort.Title:
                        visualNovels = visualNovels.OrderBy(vn =>
                        {
                            if (char.IsDigit(vn.VisualNovel.Title[0]))
                                return 0;
                            else if ((vn.VisualNovel.Title[0] >= 'a' && vn.VisualNovel.Title[0] <= 'z') || (vn.VisualNovel.Title[0] >= 'A' && vn.VisualNovel.Title[0] <= 'Z'))
                                return 1;
                            else
                                return 2;
                        }).ThenBy(vn => vn.VisualNovel.Title).ToList();
                        break;
                    default:
                        visualNovels = visualNovels.OrderBy(vn => vn.VisualNovel.Title).ToList();
                        break;
                }



                //_cache.Set(VisualNovelListCacheKey + $"_page_filtred_with_rating_{@params.Page}", visualNovels, cacheOptions);

                return visualNovels;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<VisualNovel> GetVisualNovelAsync(int id)
        {
            try
            {
                //var vn = await GetVisualNovelsAsync();

                VisualNovel visualNovel = await _db.VisualNovels
                    .Include(vn => vn.Genres)
                    //.Include(vn => vn.Tags.Where(tag => tag.SpoilerLevel <= spoilerLevel))
                    //    .ThenInclude(tag => tag.Tag)
                    .Include(vn => vn.Platforms)
                    .Include(vn => vn.Languages)
                    .Include(vn => vn.Translator)
                    .Include(vn => vn.Author)
                    .Include(vn => vn.DownloadLinks)
                    .Include(vn => vn.OtherLinks)
                    .Include(vn => vn.AnimeLinks)
                    .Include(vn => vn.RelatedNovels)
                        .ThenInclude(r => r.RelatedVisualNovel)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(vn => vn.Id == id);

                if (visualNovel == null)
                {
                    return null;
                }

                return visualNovel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> GetVisualNovelAsync(string linkName)
        {
            try
            {
                //var vn = await GetVisualNovelsAsync();

                VisualNovel visualNovel = await _db.VisualNovels
                    .Include(vn => vn.Genres)
                    //.Include(vn => vn.Tags.Where(tag => tag.SpoilerLevel <= spoilerLevel))
                    //    .ThenInclude(tag => tag.Tag)
                    .Include(vn => vn.Platforms)
                    .Include(vn => vn.Languages)
                    .Include(vn => vn.Translator)
                    .Include(vn => vn.Author)
                    .Include(vn => vn.DownloadLinks)
                    .Include(vn => vn.OtherLinks)
                    .Include(vn => vn.AnimeLinks)
                    .Include(vn => vn.RelatedNovels)
                        .ThenInclude(r => r.RelatedVisualNovel)
                    .AsSplitQuery()
                    .FirstOrDefaultAsync(vn => vn.LinkName == linkName);

                if (visualNovel == null)
                {
                    return null;
                }

                return visualNovel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<IActionResult> GetVisualNovelCoverImage(int id)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(180))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600));

                if (_cache.TryGetValue(VisualNovelListCacheKey + $"_vn_id_{id}_cover_image", out FileContentResult imageResult))
                {
                    Console.WriteLine($"Cover Image From Cache: VN Id = {id}");
                    return imageResult;
                }

                string directory = $"C:\\Users\\Oleg\\source\\repos\\VN_API\\Data\\VisualNovels\\{id}\\CoverImage\\";

                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                var file = Directory.GetFiles(directory, id.ToString() + ".*", SearchOption.AllDirectories).FirstOrDefault();

                FileInfo fileInfo = new FileInfo(file);

                byte[] imageData = File.ReadAllBytes(file);

                imageData = File.ReadAllBytes(file);

                imageResult = new FileContentResult(imageData, $"image/{fileInfo.Extension.Remove(0, 1)}");

                _cache.Set(VisualNovelListCacheKey + $"_vn_id_{id}_cover_image", imageResult, cacheOptions);

                return imageResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IActionResult> GetVisualNovelBackgroundImage(int id)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(180))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600));

                if (_cache.TryGetValue(VisualNovelListCacheKey + $"_vn_id_{id}_background_image", out FileContentResult imageResult))
                {
                    Console.WriteLine($"Background Image From Cache: VN Id = {id}");
                    return imageResult;
                }

                string directory = $"C:\\Users\\Oleg\\source\\repos\\VN_API\\Data\\VisualNovels\\{id}\\BackgroundImage\\";

                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                var file = Directory.GetFiles(directory, id.ToString() + ".*", SearchOption.AllDirectories).FirstOrDefault();

                FileInfo fileInfo = new FileInfo(file);

                byte[] imageData = File.ReadAllBytes(file);

                imageResult = new FileContentResult(imageData, $"image/{fileInfo.Extension.Remove(0, 1)}");

                _cache.Set(VisualNovelListCacheKey + $"_vn_id_{id}_background_image", imageResult, cacheOptions);

                return imageResult;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<string[]> GetVisualNovelScreenshotsPath(int id)
        {
            try
            {
                string directory = $"C:\\Users\\Oleg\\source\\repos\\VN_API\\Data\\VisualNovels\\{id}\\Screenshots\\";

                List<FileContentResult> screenshots = new List<FileContentResult>();

                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                var screenshotsPath = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

                return screenshotsPath;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<IActionResult> GetVisualNovelImageByPath(string path)
        {
            try
            {
                var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(180))
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600));

                if (_cache.TryGetValue(VisualNovelListCacheKey + $"_vn_image_by_path_{path}", out FileContentResult imageResult))
                {
                    Console.WriteLine($"Vn Image From Cache by path = {path}");
                    return imageResult;
                }

                DirectoryInfo directoryInfo = new DirectoryInfo(path);

                var file = path;

                FileInfo fileInfo = new FileInfo(file);

                byte[] imageData = File.ReadAllBytes(file);

                imageResult = new FileContentResult(imageData, $"image/{fileInfo.Extension.Remove(0, 1)}");

                _cache.Set(VisualNovelListCacheKey + $"_vn_image_by_path_{path}", imageResult, cacheOptions);

                return imageResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovel> AddVisualNovelAsync(VisualNovel vn)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var gamingPlatforms = _db.GamingPlatforms.Where(gp => vn.Platforms.Select(gp => gp.Id).Contains(gp.Id)).ToList();

                    var downloadLinks = new List<DownloadLink>();

                    foreach (var downloadLink in vn.DownloadLinks)
                    {
                        int platformId = downloadLink.GamingPlatform.Id;

                        var link = new DownloadLink()
                        {
                            Id = downloadLink.Id,
                            Url = downloadLink.Url,
                            GamingPlatform = gamingPlatforms.Where(gp => gp.Id == platformId).FirstOrDefault()
                        };

                        downloadLinks.Add(link);
                    }

                    //var tags = vn.Tags;

                    //var genres = _db.Genres.Where(genre => vn.Genres.Select(genre => genre.Id).Contains(genre.Id)).ToList();

                    //var languages = _db.Languages.Where(lang => vn.Languages.Select(lang => lang.Id).Contains(lang.Id)).ToList();

                    var sanitizer = new HtmlSanitizer();

                    var sanitizedDescription = sanitizer.Sanitize(vn.Description);

                    var visualNovel = new VisualNovel()
                    {
                        VndbId = vn.VndbId,
                        LinkName = vn.LinkName,
                        Title = vn.Title,
                        AnotherTitles = vn.AnotherTitles,

                        Status = vn.Status,
                        ReadingTime = vn.ReadingTime,

                        ReleaseYear = vn.ReleaseYear,
                        ReleaseDay = vn.ReleaseDay,
                        ReleaseMonth = vn.ReleaseMonth,

                        SteamLink = vn.SteamLink,
                        TranslateLinkForSteam = vn.TranslateLinkForSteam,

                        AddedUserName = vn.AddedUserName,
                        AdddeUserId = vn.AdddeUserId,
                        Description = sanitizedDescription,

                        DateAdded = DateTime.Now,
                        DateUpdated = DateTime.Now,

                        AnimeLinks = vn.AnimeLinks,
                        SoundtrackYoutubePlaylistLink = vn.SoundtrackYoutubePlaylistLink,
                        ScreenshotLinks = new List<string>(),

                        Translator = await _db.Translators.Where(t => vn.Translator.Select(t => t.Id).Contains(t.Id)).ToListAsync(),
                        Author = await _db.Authors.Where(a => vn.Author.Select(a => a.Id).Contains(a.Id)).ToListAsync(),
                        //Platforms = await _db.GamingPlatforms.Where(gp => gp.Id == 4).ToListAsync(),

                        //Tags = await _db.TagsMetadata,
                        Genres = await _db.Genres.Where(g => vn.Genres.Select(g => g.Id).Contains(g.Id)).ToListAsync(),
                        Languages = await _db.Languages.Where(l => vn.Languages.Select(l => l.Id).Contains(l.Id)).ToListAsync(),
                        RelatedNovels = await _db.RelatedNovels.Where(n => vn.RelatedNovels.Select(n => n.RelatedVisualNovelId).Contains(n.RelatedVisualNovelId)).ToListAsync(),
                        //Platforms = await _db.GamingPlatforms.Where(gp => vn.Platforms.Select(gp => gp.Id).Contains(gp.Id)).ToListAsync(),
                        Platforms = gamingPlatforms,
                        DownloadLinks = downloadLinks,
                        OtherLinks = vn.OtherLinks,
                    };

                    await _db.VisualNovels.AddAsync(visualNovel);

                    await _db.SaveChangesAsync();

                    // TODO Not Find Visual Novel

                    foreach (var gp in vn.Platforms)
                        AddVisualNovelToGamingPlatformAsync(gp.Id, visualNovel.Id);

                    ////foreach (var t in tags)
                    ////    AddTagMetadataToVisualNovelAsync(t.Tag.Id, visualNovel.Id, t.SpoilerLevel);

                    //foreach (var l in languages)
                    //    AddVisualNovelToLanguageAsync(l.Id, visualNovel.Id); ;

                    //foreach (var g in genres)
                    //    AddVisualNovelToGenreAsync(g.Id, visualNovel.Id);

                    //foreach (var a in vn.Author)
                    //    await AddVisualNovelToAuthorAsync(a.Id, visualNovel.Id);

                    //if (vn.Translator != null)
                    //    await AddVisualNovelToTranslatorAsync(vn.Translator.Id, visualNovel.Id);

                    //if (vn.Links != null)
                    //{
                    //    foreach (var link in vn.Links)
                    //    {
                    //        await AddDownloadLinkAsync(link);
                    //        await AddDownloadLinksToVisualNovelAsync(link.Id, visualNovel.Id);
                    //    }
                    //}     

                    //if (vn.OtherLinks != null)
                    //{
                    //    foreach (var link in vn.OtherLinks)
                    //    {
                    //        await AddOtherLinkAsync(link);
                    //        await AddOtherLinksToVisualNovelAsync(link.Id, visualNovel.Id);
                    //    }

                    //}

                    var dbvn = await _db.VisualNovels.FindAsync(visualNovel.Id);

                    if (dbvn != null)
                    {
                        await LoadOrUpdateVNDBRating(dbvn.Id);

                        if (dbvn.VndbId != null)
                            await UpdateVisualNovelTagsFromVNDB(dbvn.Id);

                        await transaction.CommitAsync();

                        return dbvn;
                    }

                    await transaction.RollbackAsync();

                    return null!;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    return null!;
                }
            }
        }

        public async Task<VisualNovel> AddVisualNovelFromJsonAsync(VisualNovel vn)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    if (vn.OtherLinks == null)
                        vn.OtherLinks = new List<OtherLink>();

                    if (vn.AnimeLinks == null)
                        vn.AnimeLinks = new List<RelatedAnimeLink>();

                    if (vn.RelatedNovels == null)
                        vn.RelatedNovels = new List<RelatedNovel>();

                    if (vn.Translator != null)
                    {
                        List<Translator> translators = vn.Translator;

                        foreach (var translator in translators)
                        {
                            var dbTranslator = await _db.Translators.Where(t => t.Name == translator.Name).FirstOrDefaultAsync();

                            if (dbTranslator != null)
                                continue;

                            _db.Translators.Add(translator);
                        }
                    }
                    
                    if (vn.Author != null)
                    {
                        List<Author> authors = vn.Author;
                        List<Author> dbAuthors = new List<Author>();
                        foreach (var author in authors)
                        {
                            var dbAuthor = await _db.Authors.Where(t => t.VndbId == author.VndbId).FirstOrDefaultAsync();

                            if (dbAuthor != null)
                                continue;

                            _db.Authors.Add(author);
                        }
                    }

                    await _db.SaveChangesAsync();

                    if (vn.Translator != null)
                    {
                        List<Translator> translators = vn.Translator;
                        List<Translator> dbTranslators = new List<Translator>();
                        foreach (var translator in translators)
                        {
                            var dbTranslator = await _db.Translators.Where(t => t.Name == translator.Name).FirstOrDefaultAsync();

                            if (dbTranslator != null)
                            {
                                dbTranslators.Add(dbTranslator);
                            }
                        }

                        vn.Translator = dbTranslators;
                    }

                    if (vn.Author != null)
                    {
                        List<Author> authors = vn.Author;
                        List<Author> dbAuthors = new List<Author>();
                        foreach (var author in authors)
                        {
                            var dbAuthor = await _db.Authors.Where(t => t.VndbId == author.VndbId).FirstOrDefaultAsync();

                            if (dbAuthor != null)
                            {
                                dbAuthors.Add(dbAuthor);
                            }
                        }

                        vn.Author = dbAuthors;
                    }

                    await transaction.CommitAsync();

                    var dbvn = await AddVisualNovelAsync(vn);

                    return dbvn;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<VisualNovel> AddCoverImageToVisualNovel(int id, IFormFile coverImage)
        {
            try
            {
                int indexLastDot = coverImage.FileName.LastIndexOf('.');

                int fileExtensionsNameLength = coverImage.FileName.Length - indexLastDot;

                string fileExtensionName = coverImage.FileName.Substring(indexLastDot + 1, fileExtensionsNameLength - 1);

                string pathToLoad = $"{id}/CoverImage/{id}.{fileExtensionName}";

                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return null;
                }

                //TODO Send file to S3

                await LoadToS3(coverImage, pathToLoad);

                vn.CoverImageLink = pathToLoad;

                _db.Entry(vn).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return vn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> AddBackgroundImageToVisualNovel(int id, IFormFile coverImage)
        {
            try
            {
                int indexLastDot = coverImage.FileName.LastIndexOf('.');

                int fileExtensionsNameLength = coverImage.FileName.Length - indexLastDot;

                string fileExtensionName = coverImage.FileName.Substring(indexLastDot + 1, fileExtensionsNameLength - 1);

                string pathToLoad = $"{id}/BackgroundImage/{id}.{fileExtensionName}";

                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return null;
                }

                await LoadToS3(coverImage, pathToLoad);

                vn.BackgroundImageLink = pathToLoad;

                _db.Entry(vn).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return vn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> AddScreenshotsToVisualNovel(int id, List<IFormFile> screenshots)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var vn = await _db.VisualNovels.FindAsync(id);

                    if (vn == null)
                    {
                        return null;
                    }

                    foreach (var iformfile in screenshots)
                    {
                        int indexLastDot = iformfile.FileName.LastIndexOf('.');

                        int fileExtensionsNameLength = iformfile.FileName.Length - indexLastDot;

                        string fileExtensionName = iformfile.FileName.Substring(indexLastDot + 1, fileExtensionsNameLength - 1);

                        string pathToLoad = $"{id}/Screenshots/{Guid.NewGuid()}.{fileExtensionName}";

                        await LoadToS3(iformfile, pathToLoad);

                        vn.ScreenshotLinks.Add(pathToLoad);

                        _db.Entry(vn).State = EntityState.Modified;

                        await _db.SaveChangesAsync();
                    }

                    //_db.Entry(vn).State = EntityState.Modified;

                    //await _db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return vn;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return null;
                }
            }
        }

        public async Task LoadToS3(IFormFile file, string path)
        {
            var fileTransferUtility = new Amazon.S3.Transfer.TransferUtility(_s3Client);

            using (var newMemoryStream = new MemoryStream())
            {
                await file.CopyToAsync(newMemoryStream);

                var uploadRequest = new Amazon.S3.Transfer.TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = path,
                    BucketName = bucketName,
                    ContentType = file.ContentType,
                    CannedACL = Amazon.S3.S3CannedACL.PublicRead // Доступ к файлу публичный
                };

                await fileTransferUtility.UploadAsync(uploadRequest);
            }
        }

        public async Task DeleteFromS3(string path)
        {
            try
            {
                var deleteObjectRequest = new Amazon.S3.Model.DeleteObjectRequest { BucketName = bucketName, Key = path };
                await _s3Client.DeleteObjectAsync(deleteObjectRequest);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteScreenshotsFolderS3(int vnId)
        {
            var vn = await _db.VisualNovels.FindAsync(vnId);

            if (vn != null && vn.ScreenshotLinks != null)
            {
                vn.ScreenshotLinks.Clear();

                string path = $"{vn.Id}/Screenshots";

                var listObjectsRequest = new Amazon.S3.Model.ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = path
                };

                var listObjectsResponse = await _s3Client.ListObjectsV2Async(listObjectsRequest);

                if (listObjectsResponse.S3Objects.Count == 0)
                {
                    Console.WriteLine("No objects found with the specified prefix.");
                    return;
                }

                // Удаляем объекты
                foreach (var s3Object in listObjectsResponse.S3Objects)
                {
                    var deleteObjectRequest = new Amazon.S3.Model.DeleteObjectRequest
                    {
                        BucketName = bucketName,
                        Key = s3Object.Key
                    };

                    await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                    Console.WriteLine($"Deleted {s3Object.Key}");
                }

                await _db.SaveChangesAsync();
            }
        }

        public async Task<VisualNovel> UpdateVisualNovelAsync([FromBody] VisualNovel vn)
        {
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var visualNovel = await _db.VisualNovels
                        .Include(dbvn => dbvn.Genres)
                        .Include(dbvn => dbvn.Tags)
                            .ThenInclude(tag => tag.Tag)
                        .Include(dbvn => dbvn.Platforms)
                        .Include(dbvn => dbvn.Languages)
                        .Include(dbvn => dbvn.Translator)
                        .Include(dbvn => dbvn.Author)
                        .Include(dbvn => dbvn.DownloadLinks)
                        .Include(dbvn => dbvn.OtherLinks)
                        .Include(dbvn => dbvn.AnimeLinks)
                        .Include(dbvn => dbvn.RelatedNovels)
                        .AsSplitQuery()
                        .FirstOrDefaultAsync(dbvn => dbvn.Id == vn.Id);

                    if (visualNovel == null)
                    {
                        return null;
                    }

                    var existingPlatformsIds = visualNovel.Platforms.Select(gp => gp.Id).ToList();
                    var newPlatformsIds = vn.Platforms.Select(gp => gp.Id);
                    var platformsToRemove = visualNovel.Platforms.Where(gp => !newPlatformsIds.Contains(gp.Id)).ToList();
                    var platformsToAdd = newPlatformsIds.Where(id => !existingPlatformsIds.Contains(id))
                                                   .Select(id => new GamingPlatform { Id = id }).ToList();

                    // Удаляем старые связи
                    foreach (var platform in platformsToRemove)
                    {
                        visualNovel.Platforms.Remove(platform);
                    }

                    // Добавляем новые связи
                    foreach (var platform in platformsToAdd)
                    {
                        _db.Entry(platform).State = EntityState.Unchanged; // Убедитесь, что автор уже существует в базе данных
                        visualNovel.Platforms.Add(platform);
                    }

                    var existingTranslatorsIds = visualNovel.Translator.Select(gp => gp.Id).ToList();
                    var newTranslatorsIds = vn.Translator.Select(gp => gp.Id);
                    var translatorsToRemove = visualNovel.Translator.Where(gp => !newTranslatorsIds.Contains(gp.Id)).ToList();
                    var translatorsToAdd = newTranslatorsIds.Where(id => !existingTranslatorsIds.Contains(id))
                                                   .Select(id => new Translator { Id = id }).ToList();

                    // Удаляем старые связи
                    foreach (var translator in translatorsToRemove)
                    {
                        visualNovel.Translator.Remove(translator);
                    }

                    // Добавляем новые связи
                    foreach (var translator in translatorsToAdd)
                    {
                        _db.Entry(translator).State = EntityState.Unchanged; // Убедитесь, что автор уже существует в базе данных
                        visualNovel.Translator.Add(translator);
                    }

                    var existingGenresIds = visualNovel.Genres.Select(gp => gp.Id).ToList();
                    var newGenresIds = vn.Genres.Select(gp => gp.Id);
                    var genresToRemove = visualNovel.Genres.Where(gp => !newGenresIds.Contains(gp.Id)).ToList();
                    var genresToAdd = newGenresIds.Where(id => !existingGenresIds.Contains(id))
                                                   .Select(id => new Genre { Id = id }).ToList();

                    // Удаляем старые связи
                    foreach (var genre in genresToRemove)
                    {
                        visualNovel.Genres.Remove(genre);
                    }

                    // Добавляем новые связи
                    foreach (var genre in genresToAdd)
                    {
                        _db.Entry(genre).State = EntityState.Unchanged; // Убедитесь, что автор уже существует в базе данных
                        visualNovel.Genres.Add(genre);
                    }

                    var existingAuthorsIds = visualNovel.Author.Select(gp => gp.Id).ToList();
                    var newAuthorsIds = vn.Author.Select(gp => gp.Id);
                    var authorsToRemove = visualNovel.Author.Where(gp => !newAuthorsIds.Contains(gp.Id)).ToList();
                    var authorsToAdd = newAuthorsIds.Where(id => !existingAuthorsIds.Contains(id))
                                                   .Select(id => new Author { Id = id }).ToList();

                    // Удаляем старые связи
                    foreach (var authors in authorsToRemove)
                    {
                        visualNovel.Author.Remove(authors);
                    }

                    // Добавляем новые связи
                    foreach (var authors in authorsToAdd)
                    {
                        _db.Entry(authors).State = EntityState.Unchanged; // Убедитесь, что автор уже существует в базе данных
                        visualNovel.Author.Add(authors);
                    }

                    var existingLanguagesIds = visualNovel.Languages.Select(gp => gp.Id).ToList();
                    var newLanguagesIds = vn.Languages.Select(gp => gp.Id);
                    var languagesToRemove = visualNovel.Languages.Where(gp => !newLanguagesIds.Contains(gp.Id)).ToList();
                    var languagesToAdd = newLanguagesIds.Where(id => !existingLanguagesIds.Contains(id))
                                                   .Select(id => new Language { Id = id }).ToList();

                    // Удаляем старые связи
                    foreach (var language in languagesToRemove)
                    {
                        visualNovel.Languages.Remove(language);
                    }

                    // Добавляем новые связи
                    foreach (var language in languagesToAdd)
                    {
                        _db.Entry(language).State = EntityState.Unchanged; // Убедитесь, что автор уже существует в базе данных
                        visualNovel.Languages.Add(language);
                    }

                    //if (visualNovel.DownloadLinks != null)
                    //{
                    //    _db.DownloadLinks.RemoveRange(visualNovel.DownloadLinks);
                    //    await _db.SaveChangesAsync();
                    //}

                    //if (visualNovel.Platforms != null)
                    //{
                    //    _db.GamingPlatforms.RemoveRange(visualNovel.Platforms);
                    //    await _db.SaveChangesAsync();
                    //}


                    //var existingRelatedNovelsIds = visualNovel.RelatedNovels.Select(gp => gp.RelatedVisualNovelId).ToList();
                    //var newRelatedNovelsIds = vn.RelatedNovels.Select(related => related.RelatedVisualNovelId);
                    //var relatedNovelsToRemove = visualNovel.RelatedNovels.Where(related => !newRelatedNovelsIds.Contains(related.RelatedVisualNovelId)).ToList();
                    //var relatedNovelsToAdd = newRelatedNovelsIds.Where(id => !existingPlatformsIds.Contains(id))
                    //                               .Select(id => new RelatedNovel 
                    //                               { 
                    //                                   //VisualNovel = visualNovel,
                    //                                   VisualNovelId = visualNovel.Id,
                    //                                   //RelatedVisualNovel = _db.VisualNovels.Find(id),
                    //                                   RelatedVisualNovelId = id,
                    //                               }).ToList();

                    //// Удаляем старые связи
                    //foreach (var novel in relatedNovelsToRemove)
                    //{
                    //    visualNovel.RelatedNovels.Remove(novel);
                    //}

                    //// Добавляем новые связи
                    //foreach (var novel in relatedNovelsToAdd)
                    //{
                    //    var relatedVisualNovel = _db.VisualNovels.AsNoTracking().FirstOrDefault(vn => vn.Id == novel.RelatedVisualNovelId);

                    //    if (relatedVisualNovel != null)
                    //    {
                    //        var newRelatedNovel = new RelatedNovel
                    //        {
                    //            VisualNovelId = visualNovel.Id,
                    //            RelatedVisualNovelId = novel.RelatedVisualNovelId,
                    //            RelatedVisualNovel = relatedVisualNovel
                    //        };

                    //        visualNovel.RelatedNovels.Add(newRelatedNovel);
                    //    }
                    //}

                    //var gamingPlatforms = _db.GamingPlatforms.Where(gp => vn.Platforms.Select(gp => gp.Id).Contains(gp.Id)).ToList();

                    //var downloadLinks = new List<DownloadLink>();

                    //foreach (var downloadLink in vn.DownloadLinks)
                    //{
                    //    int platformId = downloadLink.GamingPlatform.Id;

                    //    var link = new DownloadLink()
                    //    {
                    //        Id = Guid.NewGuid(),
                    //        Url = downloadLink.Url,
                    //        GamingPlatform = visualNovel.Platforms.Where(gp => gp.Id == platformId).FirstOrDefault()
                    //    };

                    //    downloadLinks.Add(link);
                    //}

                    //var tags = vn.Tags;

                    //var genres = _db.Genres.Where(genre => vn.Genres.Select(genre => genre.Id).Contains(genre.Id)).ToList();

                    //var languages = _db.Languages.Where(lang => vn.Languages.Select(lang => lang.Id).Contains(lang.Id)).ToList();

                    var sanitizer = new HtmlSanitizer();

                    var sanitizedDescription = sanitizer.Sanitize(vn.Description);

                    visualNovel.VndbId = vn.VndbId;
                    visualNovel.LinkName = vn.LinkName;
                    visualNovel.Title = vn.Title;
                    visualNovel.AnotherTitles = vn.AnotherTitles;
                    visualNovel.Status = vn.Status;
                    visualNovel.ReadingTime = vn.ReadingTime;
                    visualNovel.ReleaseYear = vn.ReleaseYear;
                    visualNovel.ReleaseDay = vn.ReleaseDay;
                    visualNovel.ReleaseMonth = vn.ReleaseMonth;
                    visualNovel.SteamLink = vn.SteamLink;
                    visualNovel.TranslateLinkForSteam = vn.TranslateLinkForSteam;
                    visualNovel.Description = sanitizedDescription;
                    visualNovel.DateUpdated = DateTime.Now;
                    visualNovel.SoundtrackYoutubePlaylistLink = vn.SoundtrackYoutubePlaylistLink;

                    List<OtherLink> otherLinks = new List<OtherLink>();
                    if (vn.OtherLinks != null)
                    {
                        foreach (var item in vn.OtherLinks)
                        {
                            var otherLink = new OtherLink()
                            {
                                Name = item.Name,
                                Url = item.Url
                            };

                            otherLinks.Add(otherLink);
                        }

                        _db.OtherLinks.RemoveRange(await _db.OtherLinks.Where(otherLink => otherLink.VisualNovel.Id == visualNovel.Id).ToListAsync());
                    }

                    List<RelatedAnimeLink> animeLinks = new List<RelatedAnimeLink>();
                    if (vn.AnimeLinks != null)
                    {
                        foreach (var item in vn.AnimeLinks)
                        {
                            var animeLink = new RelatedAnimeLink()
                            {
                                Name = item.Name,
                                Url = item.Url
                            };

                            animeLinks.Add(animeLink);
                        }

                        _db.AnimeLinks.RemoveRange(await _db.AnimeLinks.Where(animeLink => animeLink.VisualNovel.Id == visualNovel.Id).ToListAsync());
                    }

                    List<DownloadLink> downloadLinks = new List<DownloadLink>();

                    if (vn.DownloadLinks != null)
                    {
                        //_db.DownloadLinks.RemoveRange(await _db.DownloadLinks.Where(downloadLink => downloadLink.VisualNovel.Id == visualNovel.Id).ToListAsync());

                        foreach (var downloadLink in vn.DownloadLinks)
                        {
                            int platformId = downloadLink.GamingPlatform.Id;
                            var gpEntry = visualNovel.Platforms.Where(gp => gp.Id == platformId).FirstOrDefault();
                            if (gpEntry != null)
                            {
                                _db.Entry(gpEntry).State = EntityState.Unchanged;
                                var link = new DownloadLink()
                                {
                                    Id = Guid.NewGuid(),
                                    Url = downloadLink.Url,
                                    GamingPlatform = gpEntry
                                };

                                downloadLinks.Add(link);
                            }
                        }
                    }

                    //_db.AnimeLinks.RemoveRange(await _db.AnimeLinks.Where(animeLink => animeLink.VisualNovel.Id == visualNovel.Id).ToListAsync());


                    visualNovel.AnimeLinks = animeLinks;
                    //visualNovel.RelatedNovels = await _db.RelatedNovels.Where(n => vn.RelatedNovels.Select(n => n.RelatedVisualNovelId).Contains(n.RelatedVisualNovelId)).ToListAsync();
                    visualNovel.OtherLinks = otherLinks;
                    //visualNovel.DownloadLinks = downloadLinks;

                    //visualNovel.Translator = await _db.Translators.Where(t => vn.Translator.Select(t => t.Id).Contains(t.Id)).ToListAsync();
                    //visualNovel.Author = await _db.Authors.Where(a => vn.Author.Select(a => a.Id).Contains(a.Id)).ToListAsync();
                    //visualNovel.Genres = await _db.Genres.Where(g => vn.Genres.Select(g => g.Id).Contains(g.Id)).ToListAsync();
                    //visualNovel.Languages = await _db.Languages.Where(l => vn.Languages.Select(l => l.Id).Contains(l.Id)).ToListAsync();


                    //visualNovel.Title = vn.Title;
                    //visualNovel.VndbId = vn.VndbId;
                    //visualNovel.OriginalTitle = vn.OriginalTitle;
                    //visualNovel.Status = vn.Status;
                    //visualNovel.ReadingTime = vn.ReadingTime;
                    //visualNovel.ReleaseYear = vn.ReleaseYear;
                    //visualNovel.AddedUserName = vn.AddedUserName;
                    //visualNovel.Description = vn.Description;

                    //visualNovel.SteamLink = vn.SteamLink;
                    //visualNovel.TranslateLinkForSteam = vn.TranslateLinkForSteam;
                    ////visualNovel.Translator = vn.Translator;
                    ////visualNovel.Links = vn.Links;
                    ////visualNovel.DateAdded = vn.DateAdded;
                    //visualNovel.DateUpdated = DateTime.Now;

                    //foreach (var tag in visualNovel.Tags)
                    //    DeleteTagMetadataToVisualNovelAsync(tag.Tag.Id, visualNovel.Id);

                    //foreach (var tag in vn.Tags)
                    //    AddTagMetadataToVisualNovelAsync(tag.Tag.Id, visualNovel.Id, tag.SpoilerLevel);

                    ////visualNovel.Tags = _db.TagsMetadata.Where(t => vn.Tags.Contains(t)).ToList();
                    //visualNovel.Genres = _db.Genres.Where(g => vn.Genres.Contains(g)).ToList();
                    //visualNovel.Platforms = _db.GamingPlatforms.Where(gp => vn.Platforms.Contains(gp)).ToList();
                    //visualNovel.Languages = _db.Languages.Where(l => vn.Languages.Contains(l)).ToList();
                    //visualNovel.Author = _db.Authors.Where(a => vn.Author.Contains(a)).ToList();
                    //if (vn.Translator is not null)
                    //    visualNovel.Translator = await _db.Translators.FindAsync(vn.Translator.Id);

                    List<RelatedNovel> tempRelatedNovels = new List<RelatedNovel>();

                    tempRelatedNovels.AddRange(visualNovel.RelatedNovels);
                        
                    foreach (var novel in tempRelatedNovels)
                    {
                        var dbNovel = await _db.RelatedNovels.Where(related => related.VisualNovelId == visualNovel.Id && related.RelatedVisualNovelId == novel.RelatedVisualNovelId).FirstOrDefaultAsync();
                        visualNovel.RelatedNovels.Remove(dbNovel);
                        //await _db.SaveChangesAsync();
                    }

                    foreach (var novel in vn.RelatedNovels)
                    {
                        var relatedNovel = new RelatedNovel()
                        {
                            RelatedVisualNovelId = novel.RelatedVisualNovelId,
                            VisualNovelId = visualNovel.Id,
                        };

                        visualNovel.RelatedNovels.Add(relatedNovel);
                    }

                    //TODO Maybe change??
                    if (visualNovel.DownloadLinks != null)
                    {
                        List<DownloadLink> tempDownloadLinks = new List<DownloadLink>();

                        tempDownloadLinks.AddRange(visualNovel.DownloadLinks);

                        foreach (var downloadLink in tempDownloadLinks)
                        {
                            await DeleteDownloadLinksToVisualNovelAsync(downloadLink.Id, visualNovel.Id);
                            await DeleteDownloadLink(downloadLink);
                        }
                    }

                    foreach (var downloadLink in downloadLinks)
                    {
                        var link = await AddDownloadLinkAsync(downloadLink);
                        await AddDownloadLinksToVisualNovelAsync(link.Id, visualNovel.Id);
                    }

                    //List<OtherLink> tempOtherLinks = new List<OtherLink>();

                    //tempOtherLinks.AddRange(visualNovel.OtherLinks);

                    //foreach (var otherLink in tempOtherLinks)
                    //{
                    //    await DeleteOtherLinksToVisualNovelAsync(otherLink.Id, visualNovel.Id);
                    //    await DeleteOtherLink(otherLink);
                    //}

                    //foreach (var otherLink in vn.OtherLinks)
                    //{
                    //    var link = await AddOtherLinkAsync(otherLink);
                    //    await AddOtherLinksToVisualNovelAsync(link.Id, visualNovel.Id);
                    //}

                    _db.Entry(visualNovel).State = EntityState.Modified;

                    await _db.SaveChangesAsync();

                    await LoadOrUpdateVNDBRating(visualNovel.Id);

                    await transaction.CommitAsync();

                    return visualNovel;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return null;
                }
            }
        }

        public async Task UpdateVisualNovelLinkName(int id, string linkName)
        {
            try
            {
                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return;
                }

                _db.Entry(vn).State = EntityState.Modified;

                vn.LinkName = linkName;

                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<(bool, string)> DeleteVisualNovelAsync(VisualNovel vn)
        {
            try
            {
                var dbVn = await _db.VisualNovels.FindAsync(vn.Id);

                if (dbVn == null)
                {
                    return (false, "Visual Novel could not be found");
                }

                _db.VisualNovels.Remove(vn);
                await _db.SaveChangesAsync();

                return (true, "Visual Novel got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        public async Task<List<Genre>> GetVisualNovelGenresAsync(int id)
        {
            try
            {
                var vn = await _db.VisualNovels.FindAsync(id);
                var genres = vn.Genres;

                if (genres == null)
                {
                    return null;
                }

                return genres;
            }
            catch (Exception)
            {

                return null;
            }
        }

        public async Task<List<VisualNovel>> GetVisualNovelsWithTagAsync(int tagId)
        {
            try
            {
                List<VisualNovel> vns = await _db.VisualNovels
                    .Where(vn => vn.Tags.Any(tag => tag.Tag.Id == tagId && tag.SpoilerLevel == SpoilerLevel.None))
                    .ToListAsync();

                if (vns.Count <= 0)
                {
                    return null;
                }

                return vns;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<VisualNovel>> GetVisualNovelsWithTagAndSpoilerLevelAsync(int tagId, SpoilerLevel spoilerLevel)
        {
            try
            {
                List<VisualNovel> vns = await _db.VisualNovels
                    .Where(vn => vn.Tags.Any(tag => tag.Tag.Id == tagId && tag.SpoilerLevel <= spoilerLevel))
                    .ToListAsync();

                if (vns.Count <= 0)
                {
                    return null;
                }

                return vns;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<VisualNovel>> GetVisualNovelsWithGenreAsync(int genreId)
        {
            try
            {
                List<VisualNovel> vns = await _db.VisualNovels.Where(vn => vn.Genres.Any(genre => genre.Id == genreId)).ToListAsync();

                if (vns.Count <= 0)
                {
                    return null;
                }

                return vns;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<VisualNovel>> GetVisualNovelsWithLanguageAsync(int languageId)
        {
            try
            {
                List<VisualNovel> vns = await _db.VisualNovels.Where(vn => vn.Languages.Any(language => language.Id == languageId)).ToListAsync();

                if (vns.Count <= 0)
                {
                    return null;
                }

                return vns;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<VisualNovel>> GetVisualNovelsWithGamingPlatformAsync(int gamingPlatformId)
        {
            try
            {
                List<VisualNovel> vns = await _db.VisualNovels.Where(vn => vn.Platforms.Any(gamingPlatform => gamingPlatform.Id == gamingPlatformId)).ToListAsync();

                if (vns.Count <= 0)
                {
                    return null;
                }

                return vns;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<VisualNovel>> SearchVisualNovel(string query)
        {
            try
            {
                if (query.Length < 3 || string.IsNullOrWhiteSpace(query))
                    return null;

                query = query.ToLower();

                var vns = await _db.VisualNovels.Where(vn => vn.Title.ToLower().Contains(query)).ToListAsync(); // TODO

                return vns;
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Gaming Platform

        public async Task<List<GamingPlatform>> GetGamingPlatformsAsync()
        {
            try
            {
                return await _db.GamingPlatforms.ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<GamingPlatform>> GetGamingPlatformsWithVisualNovelsAsync()
        {
            try
            {
                return await _db.GamingPlatforms.Include(gp => gp.VisualNovels)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<GamingPlatform> GetGamingPlatformAsync(int id)
        {
            try
            {
                return await _db.GamingPlatforms.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<GamingPlatform> GetGamingPlatformWithVisualNovelsAsync(int id)
        {
            try
            {
                return _db.GamingPlatforms
                        .Include(gp => gp.VisualNovels)
                        .ToList().Find(gp => gp.Id == id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<GamingPlatform> AddGamingPlatformAsync(string gamingPlatformName)
        {
            try
            {
                var gamingPlatform = new GamingPlatform()
                {
                    Name = gamingPlatformName,
                };

                await _db.GamingPlatforms.AddAsync(gamingPlatform);
                await _db.SaveChangesAsync();
                return await _db.GamingPlatforms.FindAsync(gamingPlatform.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void AddVisualNovelToGamingPlatformAsync(int gpId, int vnId)
        {
            try
            {
                var gamingPlatform = _db.GamingPlatforms.Find(gpId);
                var vn = _db.VisualNovels.Find(vnId);

                gamingPlatform.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public void DeleteVisualNovelToGamingPlatformAsync(int gpId, int vnId)
        {
            try
            {
                var gamingPlatform = _db.GamingPlatforms.Find(gpId);
                var vn = _db.VisualNovels.Find(vnId);
                gamingPlatform.VisualNovels.Remove(vn);
                //await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        public async Task<GamingPlatform> UpdateGamingPlatformAsync(int gamingPlatformId, string gamingPlatformName)
        {
            try
            {
                var gamingPlatform = await _db.GamingPlatforms.FindAsync(gamingPlatformId);

                if (gamingPlatform == null)
                {
                    return null;
                }

                gamingPlatform.Name = gamingPlatformName;

                _db.Entry(gamingPlatform).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return gamingPlatform;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool, string)> DeleteGamingPlatformAsync(GamingPlatform gamingPlatform)
        {
            try
            {
                var dbVn = await _db.GamingPlatforms.FindAsync(gamingPlatform.Id);

                if (dbVn == null)
                {
                    return (false, "Gaming platform could not be found");
                }

                _db.GamingPlatforms.Remove(gamingPlatform);
                await _db.SaveChangesAsync();

                return (true, "Gaming platform got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        #endregion

        #region Language

        public async Task<List<Language>> GetLanguagesAsync()
        {
            try
            {
                return await _db.Languages.ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Language> GetLanguageAsync(int id)
        {
            try
            {
                return await _db.Languages.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Language> AddLanguageAsync(string languageName)
        {
            try
            {
                var language = new Language()
                {
                    Name = languageName,
                };

                await _db.Languages.AddAsync(language);
                await _db.SaveChangesAsync();
                return await _db.Languages.FindAsync(language.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void AddVisualNovelToLanguageAsync(int languageId, int vnId)
        {
            try
            {
                var language = _db.Languages.Find(languageId);
                var vn = _db.VisualNovels.Find(vnId);

                language.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        public void DeleteVisualNovelToLanguageAsync(int languageId, int vnId)
        {
            try
            {
                var language = _db.Languages.Find(languageId);
                var vn = _db.VisualNovels.Find(vnId);

                language.VisualNovels.Remove(vn);
                //await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<Language> UpdateLanguageAsync(int languageId, string languageName)
        {
            try
            {
                var language = await _db.Languages.FindAsync(languageId);

                if (language == null)
                {
                    return null;
                }

                language.Name = languageName;

                _db.Entry(language).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return language;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool, string)> DeleteLanguageAsync(Language language)
        {
            try
            {
                var dbVn = await _db.Languages.FindAsync(language.Id);

                if (dbVn == null)
                {
                    return (false, "Language could not be found");
                }

                _db.Languages.Remove(language);
                await _db.SaveChangesAsync();

                return (true, "Language got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        #endregion

        #region Genre

        public async Task<List<Genre>> GetGenresAsync()
        {
            try
            {
                return await _db.Genres.ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Genre> GetGenreAsync(int id)
        {
            try
            {
                return await _db.Genres.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Genre> AddGenreAsync(string genreName)
        {
            try
            {
                var dbGenre = new Genre
                {
                    Name = genreName,
                };

                _db.Genres.Add(dbGenre);
                _db.SaveChanges();

                return await _db.Genres.FindAsync(dbGenre.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void AddVisualNovelToGenreAsync(int genreId, int vnId)
        {
            try
            {
                var genre = _db.Genres.Find(genreId);
                var vn = _db.VisualNovels.Find(vnId);

                genre.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {

            }
        }

        public void DeleteVisualNovelToGenreAsync(int genreId, int vnId)
        {
            try
            {
                var genre = _db.Genres.Find(genreId);
                var vn = _db.VisualNovels.Find(vnId);

                genre.VisualNovels.Remove(vn);
                //await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<Genre> UpdateGenreAsync(int genreId, string genreName)
        {
            try
            {
                var genre = await _db.Genres.FindAsync(genreId);

                if (genre == null)
                {
                    return null;
                }

                genre.Name = genreName;

                _db.Entry(genre).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return genre;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool, string)> DeleteGenreAsync(Genre genre)
        {
            try
            {
                var dbVn = await _db.Genres.FindAsync(genre.Id);

                if (dbVn == null)
                {
                    return (false, "Genre could not be found");
                }

                _db.Genres.Remove(genre);
                await _db.SaveChangesAsync();

                return (true, "Genre got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        #endregion

        #region Tag

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            try
            {
                var tags = await _db.Tags.Where(t => t.Applicable == true).ToListAsync(); ;

                var listTags = tags
                    .OrderBy(t =>
                    {
                        if ((t.Name[0] >= 'а' && t.Name[0] <= 'я') || (t.Name[0] >= 'А' && t.Name[0] <= 'Я'))
                        {
                            return 0;
                        }
                        else if ((t.Name[0] >= 'a' && t.Name[0] <= 'z') || (t.Name[0] >= 'A' && t.Name[0] <= 'Z'))
                        {
                            return 1;
                        }
                        else if (char.IsDigit(t.Name[0]))
                        {
                            return 2;
                        }
                        else
                        {
                            return 3;
                        }
                    }).ThenBy(t => t.Name).ToList();

                return listTags;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(List<Tag>, int)> GetTagsAsync(PaginationParams @params)
        {
            try
            {
                var position = (@params.Page - 1) * @params.ItemsPerPage;

                var paginatedTags = await
                    _db.Tags
                        .OrderBy(t => t.Id)
                        .Where(t => t.Applicable == true)
                        .Skip(position)
                        .Take(@params.ItemsPerPage)
                        .ToListAsync();

                var count = await _db.Tags.Where(t => t.Applicable == true).CountAsync();

                return (paginatedTags, count);
            }
            catch (Exception ex)
            {
                return (null, -1);
            }
        }

        public async Task<(List<Tag>, int)> SearchTags(PaginationParams @params, string query)
        {
            try
            {
                var position = (@params.Page - 1) * @params.ItemsPerPage;

                if (query.Length < 3 || string.IsNullOrWhiteSpace(query))
                    return (null, -1);

                query = query.ToLower();

                var tags = await
                    _db.Tags
                    .OrderBy(t => t.Id)
                    .Where(t => t.Applicable == true && t.Name.ToLower().Contains(query) || t.EnglishName.ToLower().Contains(query))
                    .Skip(position)
                    .Take(@params.ItemsPerPage)
                    .ToListAsync(); ; // TODO

                var count = await
                    _db.Tags
                    .Where(t => t.Applicable == true && t.Name.ToLower().Contains(query) || t.EnglishName.ToLower().Contains(query))
                    .CountAsync();

                return (tags, count);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Tag> GetTagAsync(int id)
        {
            try
            {
                return await _db.Tags.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Tag> AddTagAsync(string tagName, string description)
        {
            try
            {
                var tag = new Tag()
                {
                    Name = tagName,
                    Description = description,
                };

                await _db.Tags.AddAsync(tag);
                await _db.SaveChangesAsync();
                return await _db.Tags.FindAsync(tag.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Tag> UpdateTagAsync(int tagId, string tagName)
        {
            try
            {
                var tag = await _db.Tags.FindAsync(tagId);

                if (tag == null)
                {
                    return null;
                }

                tag.Name = tagName;

                _db.Entry(tag).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return tag;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool, string)> DeleteTagAsync(Tag tag)
        {
            try
            {
                var dbVn = await _db.Tags.FindAsync(tag.Id);

                if (dbVn == null)
                {
                    return (false, "Tag could not be found");
                }

                _db.Tags.Remove(tag);
                await _db.SaveChangesAsync();

                return (true, "Tag got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        #endregion

        #region TagMetadata

        public async Task<TagMetadata> GetTagMetadata(Guid id)
        {
            try
            {
                return await _db.TagsMetadata.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<TagMetadata>> GetTagMetadataAsync(int tagId)
        {
            try
            {
                return await _db.TagsMetadata.Where(tag => tag.Tag.Id == tagId).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<TagMetadata>> GetVisualNovelTagsMetadataAsync(int visualNovelId, SpoilerLevel spoilerLevel)
        {
            try
            {
                var tags = await _db.TagsMetadata
                    .Include(t => t.Tag)
                    .Where(t => t.VisualNovel.Id == visualNovelId)
                    .Where(t => t.SpoilerLevel <= spoilerLevel)
                    .ToListAsync();

                return tags;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<TagMetadata> AddTagMetadataAsync(int tagId, int visualNovelId, SpoilerLevel spoilerLevel)
        {
            try
            {
                var tagMetadata = new TagMetadata()
                {
                    Id = Guid.NewGuid(),
                    Tag = await _db.Tags.FindAsync(tagId),
                    VisualNovel = await _db.VisualNovels.FindAsync(visualNovelId),
                    SpoilerLevel = spoilerLevel,
                };

                await _db.TagsMetadata.AddAsync(tagMetadata);
                await _db.SaveChangesAsync();
                return await _db.TagsMetadata.FindAsync(tagMetadata.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public void AddTagMetadataToVisualNovelAsync(int tagId, int vnId, SpoilerLevel spoilerLevel)
        {
            try
            {
                var tag = _db.Tags.Find(tagId);
                var vn = _db.VisualNovels.Find(vnId);

                if (vn != null && tag != null)
                {
                    TagMetadata tagMetadata = new TagMetadata()
                    {
                        Id = Guid.NewGuid(),
                        Tag = tag,
                        VisualNovel = vn,
                        SpoilerLevel = spoilerLevel,
                    };

                    _db.TagsMetadata.Add(tagMetadata);

                    //tag.VisualNovels.Add(vn);
                }

                //await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public void DeleteTagMetadataToVisualNovelAsync(int tagId, int vnId)
        {
            try
            {
                var tag = _db.TagsMetadata.Where(tag => tag.VisualNovel.Id == vnId && tag.Tag.Id == tagId).First();

                if (tag != null)
                    _db.TagsMetadata.Remove(tag);

                //await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<TagMetadata> UpdateTagMetadataAsync(Guid id, int tagId, int visualNovelId, SpoilerLevel spoilerLevel)
        {
            try
            {
                var tagMetadata = await _db.TagsMetadata.FindAsync(id);

                if (tagMetadata == null)
                {
                    return null;
                }

                tagMetadata.Tag = await _db.Tags.FindAsync(tagId);
                tagMetadata.VisualNovel = await _db.VisualNovels.FindAsync(visualNovelId);
                tagMetadata.SpoilerLevel = spoilerLevel;

                _db.Entry(tagMetadata).State = EntityState.Modified;
                await _db.SaveChangesAsync();

                return tagMetadata;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<(bool, string)> DeleteTagMetadataAsync(TagMetadata tagMetadata)
        {
            try
            {
                var dbTagMetadata = await _db.TagsMetadata.FindAsync(tagMetadata.Id);

                if (dbTagMetadata == null)
                {
                    return (false, "TagMetadata could not be found");
                }

                _db.TagsMetadata.Remove(dbTagMetadata);

                await _db.SaveChangesAsync();

                return (true, "TagMetadata got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        #endregion

        #region VisualNovelRating

        public async Task<List<VisualNovelRating>> GetVisualNovelRatingsAsync()
        {
            try
            {
                return await _db.Rating.ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovelRating> GetVisualNovelRatingAsync(Guid id)
        {
            try
            {
                return await _db.Rating.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<VisualNovelRating>> GetVisualNovelRatingByVisualNovelIdAsync(int id)
        {
            try
            {
                return await _db.Rating.Where(rating => rating.VisualNovelId == id).ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(double, int)> GetVisualNovelAverageRatingWithCount(int id)
        {
            try
            {
                if (_db.Rating.Where(r => r.VisualNovelId == id).Any())
                {
                    var cortage = (await _db.Rating
                    .Where(rating => rating.VisualNovelId == id)
                    .AverageAsync(rating => rating.Rating),
                 await _db.Rating
                    .Where(rating => rating.VisualNovelId == id)
                    .CountAsync());

                    return cortage;
                }

                return (-1, -1);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovelRating> AddVisualNovelRatingAsync(VisualNovelRating vnRating)
        {
            try
            {
                var visualNovel = await _db.VisualNovels.FindAsync(vnRating.VisualNovelId);

                if (visualNovel == null)
                {
                    return null;
                }

                var dbRating = await _db.Rating.Where(r => r.UserId == vnRating.UserId && r.VisualNovelId == vnRating.VisualNovelId).FirstOrDefaultAsync();

                if (dbRating != null)
                {
                    return null;
                }

                VisualNovelRating rating = new VisualNovelRating()
                {
                    Id = Guid.NewGuid(),
                    UserId = vnRating.UserId,
                    VisualNovelId = vnRating.VisualNovelId,
                    Rating = vnRating.Rating,
                    AddingTime = DateTime.Now,
                };

                _db.Rating.Add(rating);

                await _db.SaveChangesAsync();

                return await _db.Rating.FindAsync(rating.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovelRating> UpdateVisualNovelRatingAsync(Guid id, int vnRating)
        {
            try
            {
                VisualNovelRating rating = await _db.Rating.FindAsync(id);

                if (rating == null)
                {
                    return null;
                }

                rating.Rating = vnRating;

                _db.Entry(rating).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return rating;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool, string)> DeleteVisualNovelRatingAsync(VisualNovelRating vnRating)
        {
            try
            {
                VisualNovelRating rating = await _db.Rating.FindAsync(vnRating.Id);

                if (rating == null)
                {
                    return (false, "Rating could not be found");
                }

                _db.Rating.Remove(rating);

                await _db.SaveChangesAsync();

                return (true, "Rating got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        public async Task<int> GetVisualNovelUserRatingAsync(Guid userId, int visualNovelId)
        {
            try
            {
                return await _db.Rating
                    .Where(rating => rating.UserId == userId && rating.VisualNovelId == visualNovelId)
                    .Select(rating => rating.Rating)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public async Task<VisualNovelRating> UpdateRatingByUserAsync(Guid userId, int visualNovelId, int rating)
        {
            try
            {
                VisualNovelRating dbRating = await _db.Rating
                    .Where(r => r.UserId == userId && r.VisualNovelId == visualNovelId)
                    .FirstOrDefaultAsync();

                if (dbRating == null)
                {
                    return null;
                }

                dbRating.Rating = rating;

                _db.Entry(dbRating).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return dbRating;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool, string)> RemoveRatingByUserAsync(Guid userId, int visualNovelId)
        {
            try
            {
                VisualNovelRating rating = await _db.Rating
                    .Where(rating => rating.UserId == userId && rating.VisualNovelId == visualNovelId)
                    .FirstOrDefaultAsync();

                if (rating == null)
                {
                    return (false, "Rating could not be found");
                }

                _db.Rating.Remove(rating);

                await _db.SaveChangesAsync();

                return (true, "Rating got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        #endregion

        #region Author
        public async Task<List<Author>> GetAuthorsAsync()
        {
            try
            {
                return await _db.Authors.ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Author> GetAuthorAsync(int id)
        {
            try
            {
                return await _db.Authors.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<Author> AddAuthorAsync(Author author)
        {
            try
            {
                //var visualNovel = await _db.VisualNovels.FindAsync(vnRating.VisualNovelId);

                //if (visualNovel == null)
                //{
                //    return null;
                //}

                Author newAuthor = new Author()
                {
                    Name = author.Name,
                    VndbId = author.VndbId,
                    Source = author.Source,
                };

                _db.Authors.Add(newAuthor);

                await _db.SaveChangesAsync();

                return await _db.Authors.FindAsync(newAuthor.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task AddVisualNovelToAuthorAsync(int authorId, int vnId)
        {
            try
            {
                var author = await _db.Authors.FindAsync(authorId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                author.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task DeleteVisualNovelToAuthorAsync(int authorId, int vnId)
        {
            try
            {
                var author = await _db.Authors.FindAsync(authorId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                author.VisualNovels.Remove(vn);
                //await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<Author> UpdateAuthorAsync(int authorId, Author author)
        {
            try
            {
                Author dbAuthor = await _db.Authors.FindAsync(author.Id);

                if (dbAuthor == null)
                {
                    return null;
                }

                dbAuthor = author;

                _db.Entry(dbAuthor).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return dbAuthor;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool, string)> DeleteAuthorAsync(Author author)
        {
            try
            {
                Author dbAuthor = await _db.Authors.FindAsync(author.Id);

                if (dbAuthor == null)
                {
                    return (false, "Author could not be found");
                }

                _db.Authors.Remove(dbAuthor);

                await _db.SaveChangesAsync();

                return (true, "Author got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        #endregion

        #region Translator
        public async Task<List<Translator>> GetTranslatorsAsync()
        {
            try
            {
                return await _db.Translators.ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Translator> GetTranslatorAsync(int id)
        {
            try
            {
                return await _db.Translators.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<Translator> AddTranslatorAsync(Translator translator)
        {
            try
            {
                //var visualNovel = await _db.VisualNovels.FindAsync(vnRating.VisualNovelId);

                //if (visualNovel == null)
                //{
                //    return null;
                //}

                Translator newTranslator = new Translator()
                {
                    Name = translator.Name,
                    Source = translator.Source,
                };

                _db.Translators.Add(newTranslator);

                await _db.SaveChangesAsync();

                return await _db.Translators.FindAsync(newTranslator.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task AddVisualNovelToTranslatorAsync(int translatorId, int vnId)
        {
            try
            {
                var translator = await _db.Translators.FindAsync(translatorId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                translator.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Translator> UpdateTranslatorAsync(int translatorId, Translator translator)
        {
            try
            {
                Translator dbTranslator = await _db.Translators.FindAsync(translator.Id);

                if (dbTranslator == null)
                {
                    return null;
                }

                dbTranslator = translator;

                _db.Entry(dbTranslator).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return dbTranslator;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task DeleteVisualNovelToTranslatorAsync(int translatorId, int vnId)
        {
            try
            {
                var translator = await _db.Translators.FindAsync(translatorId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                translator.VisualNovels.Remove(vn);
                //await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public async Task<(bool, string)> DeleteTranslatorAsync(Translator translator)
        {
            try
            {
                Translator dbTranslator = await _db.Translators.FindAsync(translator.Id);

                if (dbTranslator == null)
                {
                    return (false, "Translator could not be found");
                }

                _db.Translators.Remove(dbTranslator);

                await _db.SaveChangesAsync();

                return (true, "Translator got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }
        #endregion
        #region Download Links
        public async Task<List<DownloadLink>> GetDownloadLinks()
        {
            try
            {
                return await _db.DownloadLinks.ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<DownloadLink>> GetDownloadLinksForVisualNovel(int id)
        {
            try
            {
                return await _db.DownloadLinks.Where(link => link.VisualNovel.Id == id).ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<DownloadLink> AddDownloadLinkAsync(DownloadLink downloadLink)
        {
            try
            {
                DownloadLink link = new DownloadLink()
                {
                    Id = downloadLink.Id,
                    Url = downloadLink.Url,
                    GamingPlatform = await GetGamingPlatformAsync(downloadLink.GamingPlatform.Id),
                };

                _db.DownloadLinks.Add(link);

                //await _db.SaveChangesAsync();

                return await _db.DownloadLinks.FindAsync(link.Id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddDownloadLinksToVisualNovelAsync(Guid downloadLinkId, int vnId)
        {
            try
            {
                var downloadLink = await _db.DownloadLinks.FindAsync(downloadLinkId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                vn.DownloadLinks.Add(downloadLink);
                //await _db.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task DeleteDownloadLinksToVisualNovelAsync(Guid downloadLinkId, int vnId)
        {
            try
            {
                var downloadLink = await _db.DownloadLinks.FindAsync(downloadLinkId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                vn.DownloadLinks.Remove(downloadLink);
                //await _db.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<DownloadLink> UpdateDownloadLink(Guid downloadLinkId, DownloadLink downloadLink)
        {
            try
            {
                DownloadLink dbDownloadLink = await _db.DownloadLinks.FindAsync(downloadLinkId);

                if (dbDownloadLink == null)
                {
                    return null;
                }

                dbDownloadLink = downloadLink;

                _db.Entry(dbDownloadLink).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return dbDownloadLink;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool, string)> DeleteDownloadLink(DownloadLink downloadLink)
        {
            try
            {
                DownloadLink dbDownloadLink = await _db.DownloadLinks.FindAsync(downloadLink.Id);

                if (dbDownloadLink == null)
                {
                    return (false, "Download link could not be found");
                }

                _db.DownloadLinks.Remove(dbDownloadLink);

                //await _db.SaveChangesAsync();

                return (true, "Download link got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }
        #endregion
        #region Other Links

        public async Task<List<OtherLink>> GetOtherLinks()
        {
            try
            {
                return await _db.OtherLinks.ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<OtherLink>> GetOtherLinksForVisualNovel(int id)
        {
            try
            {
                return await _db.OtherLinks.Where(link => link.VisualNovel.Id == id).ToListAsync();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<OtherLink> AddOtherLinkAsync(OtherLink otherLink)
        {
            try
            {
                _db.OtherLinks.Add(otherLink);

                //await _db.SaveChangesAsync();

                return await _db.OtherLinks.FindAsync(otherLink.Id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddOtherLinksToVisualNovelAsync(Guid otherLinkId, int vnId)
        {
            try
            {
                var otherLink = await _db.OtherLinks.FindAsync(otherLinkId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                vn.OtherLinks.Add(otherLink);
                //await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteOtherLinksToVisualNovelAsync(Guid otherLinkId, int vnId)
        {
            try
            {
                var otherLink = await _db.OtherLinks.FindAsync(otherLinkId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                vn.OtherLinks.Remove(otherLink);
                //await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<OtherLink> UpdateOtherLink(Guid otherLinkId, OtherLink otherLink)
        {
            try
            {
                OtherLink dbOtherLink = await _db.OtherLinks.FindAsync(otherLinkId);

                if (dbOtherLink == null)
                {
                    return null;
                }

                dbOtherLink = otherLink;

                _db.Entry(dbOtherLink).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return dbOtherLink;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool, string)> DeleteOtherLink(OtherLink otherLink)
        {
            try
            {
                OtherLink dbOtherLink = await _db.OtherLinks.FindAsync(otherLink.Id);

                if (dbOtherLink == null)
                {
                    return (false, "Other link could not be found");
                }

                _db.OtherLinks.Remove(dbOtherLink);

                //await _db.SaveChangesAsync();

                return (true, "Other link got deleted");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }
        #endregion
        #region Visual Novel List
        public async Task<List<VisualNovelList>> GetVisualNovelLists(string userId, bool showPrivate)
        {
            try
            {
                var userLists = new List<VisualNovelList>();

                if (showPrivate)
                {
                    userLists = await _db.VisualNovelLists
                        .Include(list => list.ListType)
                        .OrderBy(list => list.ListType.Id)
                        .Where(list => list.UserId == userId)
                        .ToListAsync();

                }
                else
                {
                    userLists = await _db.VisualNovelLists
                        .Include(list => list.ListType)
                        .OrderBy(list => list.ListType.Id)
                        .Where(list => list.UserId == userId && list.IsPrivate == false)
                        .ToListAsync();
                }

                return userLists;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<VisualNovelListEntry>> GetVisualNovelInAnyUserList(string userId, int visualNovelId)
        {
            try
            {
                var entry = await _db.VisualNovelListEntries
                    .Include(entry => entry.VisualNovelList)
                        .ThenInclude(list => list.ListType)
                    .Where(entry => entry.VisualNovelList.UserId == userId && entry.VisualNovel.Id == visualNovelId).ToListAsync();

                return entry;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovelList> GetVisualNovelList(int listId)
        {
            try
            {
                var list = await _db.VisualNovelLists
                    .Include(list => list.ListType)
                    .Where(list => list.Id == listId)
                    .FirstOrDefaultAsync();

                if (list == null)
                {
                    return null;
                }

                return list;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<VisualNovelListEntry>> GetUserVisualNovelsInLists(string userId, bool showPrivate)
        {
            try
            {
                var userVNInList = new List<VisualNovelListEntry?>();

                if (showPrivate)
                {
                    userVNInList = await _db.VisualNovelListEntries
                        .Include(listEntry => listEntry.VisualNovelList)
                        .Include(listEntry => listEntry.VisualNovel)
                        .Where(listEntry => listEntry.VisualNovelList.UserId == userId)
                        .GroupBy(listEntry => listEntry.VisualNovel.Id)
                        .Select(g => g.OrderByDescending(entry => entry.AddingTime).FirstOrDefault())
                        .ToListAsync();
                }
                else
                {
                    userVNInList = await _db.VisualNovelListEntries
                        .Include(listEntry => listEntry.VisualNovelList)
                        .Include(listEntry => listEntry.VisualNovel)
                        .Where(listEntry => listEntry.VisualNovelList.UserId == userId && listEntry.VisualNovelList.IsPrivate == showPrivate)
                        .GroupBy(listEntry => listEntry.VisualNovel.Id)
                        .Select(g => g.OrderByDescending(entry => entry.AddingTime).FirstOrDefault())
                        .ToListAsync();
                }

                if (userVNInList == null)
                {
                    return null;
                }


                return userVNInList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<VisualNovelListEntry>> GetVisualNovelsInList(string userId, int listId)
        {
            try
            {
                var userVNInList = new List<VisualNovelListEntry>();

                var list = await _db.VisualNovelLists.FindAsync(listId);

                if (list == null || list.UserId != userId)
                {
                    return null;
                }

                //var position = (@params.Page - 1) * @params.ItemsPerPage;

                userVNInList = await _db.VisualNovelListEntries
                    .Include(listEntry => listEntry.VisualNovel)
                    .Where(listEntry => listEntry.VisualNovelList == list)
                    //.Skip(position)
                    //.Take(@params.ItemsPerPage)
                    .ToListAsync();

                return userVNInList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovelList> UpdateVisualNovelList(string userId, int visualNovelListId, VisualNovelList visualNovelList)
        {
            try
            {
                var dbList = await _db.VisualNovelLists.FindAsync(visualNovelListId);

                if (dbList == null || dbList.UserId != userId)
                {
                    return null;
                }


                dbList.IsPrivate = visualNovelList.IsPrivate;
                dbList.Name = visualNovelList.Name;

                _db.Entry(dbList).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return dbList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool, string)> CreateBaseLists(string userId)
        {
            try
            {
                List<VisualNovelListType> baseListTypes = await _db.ListTypes.ToListAsync();

                if (_db.VisualNovelLists.Where(list => list.UserId == userId).Any())
                {
                    return (false, "User have base lists");
                }

                foreach (var type in baseListTypes)
                {
                    var list = new VisualNovelList()
                    {
                        IsCustom = false,
                        IsPrivate = false,
                        ListType = type,
                        UserId = userId,
                        VisualNovelListEntries = new List<VisualNovelListEntry>(),
                        Name = type.Name,
                    };

                    _db.VisualNovelLists.Add(list);
                }

                await _db.SaveChangesAsync();

                return (true, $"Base lists added to user: {userId}");
            }
            catch (Exception ex)
            {
                return (false, $"Error, {ex.Message}");
            }
        }

        public async Task<VisualNovelList> CreateCustomList(string userId, VisualNovelList visualNovelList)
        {
            try
            {
                var list = new VisualNovelList()
                {
                    IsCustom = true,
                    IsPrivate = visualNovelList.IsPrivate,
                    ListType = null,
                    UserId = userId,
                    VisualNovelListEntries = new List<VisualNovelListEntry>(),
                    Name = visualNovelList.Name,
                };

                var dblist = _db.VisualNovelLists.Where(list => list.UserId == userId && list.Name == visualNovelList.Name).FirstOrDefault();

                if (dblist != null)
                {
                    return null;
                }

                _db.VisualNovelLists.Add(list);

                await _db.SaveChangesAsync();

                return _db.VisualNovelLists.Where(list => list.UserId == userId && list.Name == visualNovelList.Name).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<(bool, string)> AddToList(string userId, int visualNovelListId, int visualNovelId)
        {
            try
            {
                var list = await _db.VisualNovelLists.FindAsync(visualNovelListId);
                var vn = await _db.VisualNovels.FindAsync(visualNovelId);

                if (list == null || list.UserId != userId)
                {
                    return (false, "Not Found List or current user haven't that list");
                }

                if (vn == null)
                {
                    return (false, "Visual novel couldn't exist");
                }

                var entry = new VisualNovelListEntry()
                {
                    VisualNovel = vn,
                    VisualNovelList = list,
                    AddingTime = DateTime.Now,
                };

                _db.VisualNovelListEntries.Add(entry);

                await _db.SaveChangesAsync();

                return (true, $"Visual Novel add to list: {visualNovelListId} is successful");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<(bool, string)> RemoveFromList(string userId, int visualNovelListId, int visualNovelId)
        {
            try
            {
                var list = await _db.VisualNovelLists.FindAsync(visualNovelListId);
                var vn = await _db.VisualNovels.FindAsync(visualNovelId);


                if (list == null || list.UserId != userId)
                {
                    return (false, "Not Found List or current user haven't that list");
                }

                if (vn == null)
                {
                    return (false, "Visual novel couldn't exist");
                }

                var entry = await _db.VisualNovelListEntries.Where(e => e.VisualNovel == vn && e.VisualNovelList == list).FirstOrDefaultAsync();

                if (entry == null)
                {
                    return (false, "Not Found Entry");
                }

                _db.VisualNovelListEntries.Remove(entry);

                await _db.SaveChangesAsync();

                return (true, $"Visual Novel remove from list: {visualNovelListId}");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(bool, string)> DeleteList(string userId, int visualNovelListId)
        {
            try
            {
                var list = await _db.VisualNovelLists.FindAsync(visualNovelListId);

                if (list == null || list.UserId != userId)
                {
                    return (false, "Not Found List or current user haven't that list");
                }

                _db.VisualNovelLists.Remove(list);

                await _db.SaveChangesAsync();

                return (true, $"Visual Novel List {visualNovelListId} was removed");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<VisualNovelListType> AddListType(string name, bool isMutuallyExclusive)
        {
            try
            {
                var listType = new VisualNovelListType()
                {
                    Name = name,
                };

                await _db.ListTypes.AddAsync(listType);

                await _db.SaveChangesAsync();

                return listType;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}