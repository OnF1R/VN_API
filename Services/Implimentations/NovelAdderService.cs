using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VN_API.Database;
using VN_API.Extensions;
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


        public NovelAdderService(ApplicationContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;

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
                Sort sort
            )
        {
            try
            {
                //var cacheOptions = new MemoryCacheEntryOptions()
                //.SetSlidingExpiration(TimeSpan.FromSeconds(180))
                //.SetAbsoluteExpiration(TimeSpan.FromSeconds(3600));

                //if (_cache.TryGetValue(VisualNovelListCacheKey + $"_page_filtred_with_rating_{@params.Page}", out List<VisualNovelWithRating> visualNovels))
                //    return visualNovels;

                List<VisualNovelWithRating> visualNovels;

                var vns = _db.VisualNovels.AsQueryable();

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

                switch (sort)
                {
                    case Sort.DateUpdatedDescending:
                        visualNovels = visualNovels.OrderByDescending(vn => vn.VisualNovel.DateUpdated).ToList();
                        break;
                    case Sort.DateUpdatedAscending:
                        visualNovels = visualNovels.OrderBy(vn => vn.VisualNovel.DateUpdated).ToList();
                        break;
                    case Sort.ReleaseDateDescending:
                        visualNovels = visualNovels.OrderByDescending(vn => vn.VisualNovel.ReleaseYear).ToList();
                        break;
                    case Sort.ReleaseDateAscending:
                        visualNovels = visualNovels.OrderBy(vn => vn.VisualNovel.ReleaseYear).ToList();
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
        public async Task<VisualNovel> GetVisualNovelAsync(int id, SpoilerLevel spoilerLevel)
        {
            try
            {
                //var vn = await GetVisualNovelsAsync();

                VisualNovel visualNovel = await _db.VisualNovels
                    .Include(vn => vn.Genres)
                    .Include(vn => vn.Tags.Where(tag => tag.SpoilerLevel <= spoilerLevel))
                        .ThenInclude(tag => tag.Tag)
                    .Include(vn => vn.Platforms)
                    .Include(vn => vn.Languages)
                    .Include(vn => vn.Translator)
                    .Include(vn => vn.Author)
                    .Include(vn => vn.Links)
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
            try
            {
                var gamingPlatforms = _db.GamingPlatforms.Where(gp => vn.Platforms.Contains(gp)).ToList();

                var tags = vn.Tags;

                var genres = _db.Genres.Where(genre => vn.Genres.Contains(genre)).ToList();

                var languages = _db.Languages.Where(lang => vn.Languages.Contains(lang)).ToList();

                var visualNovel = new VisualNovel()
                {
                    Title = vn.Title,
                    VndbId = vn.VndbId,
                    CoverImagePath = null,
                    OriginalTitle = vn.OriginalTitle,
                    ReadingTime = vn.ReadingTime,
                    Status = vn.Status,
                    SteamLink = vn.SteamLink,
                    
                    ReleaseYear = vn.ReleaseYear,

                    DateAdded = DateTime.Now,
                    DateUpdated = DateTime.Now,

                    AddedUserName = vn.AddedUserName,
                    Description = vn.Description,

                    Translator = null,
                    Author = null,

                    Platforms = null,
                    Tags = null,
                    Genres = null,
                    Languages = null,
                    Links = vn.Links,
                };

                await _db.VisualNovels.AddAsync(visualNovel);

                await _db.SaveChangesAsync();

                // TODO Not Find Visual Novel

                foreach (var gp in gamingPlatforms)
                    AddVisualNovelToGamingPlatformAsync(gp.Id, visualNovel.Id);

                foreach (var t in tags)
                    AddTagMetadataToVisualNovelAsync(t.Tag.Id, visualNovel.Id, t.SpoilerLevel);

                foreach (var l in languages)
                    AddVisualNovelToLanguageAsync(l.Id, visualNovel.Id); ;

                foreach (var g in genres)
                    AddVisualNovelToGenreAsync(g.Id, visualNovel.Id);

                foreach (var a in vn.Author)
                    await AddVisualNovelToAuthorAsync(a.Id, visualNovel.Id);
                
                if (vn.Translator != null)
                    await AddVisualNovelToTranslatorAsync(vn.Translator.Id, visualNovel.Id);

                await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(visualNovel.Id);
            }
            catch (Exception ex)
            {
                if (_db.VisualNovels.Find(vn.Id) != null)
                    _db.VisualNovels.Remove(vn);

                return null;
            }
        }
        public async Task<VisualNovel> AddCoverImageToVisualNovel(int id, IFormFile coverImage)
        {
            try
            {
                ImageType type;

                string directory = $"C:\\Users\\Oleg\\source\\repos\\VN_API\\Data\\VisualNovels\\{id}\\CoverImage\\";

                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return null;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    coverImage.CopyTo(ms);
                    var byteImage = ms.ToArray();

                    type = ImageFormat.GetImageFormat(byteImage);

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    if (Directory.GetFiles(directory).Length > 0)
                    {
                        foreach (FileInfo file in directoryInfo.GetFiles())
                        {
                            file.Delete();
                        }
                    }

                    vn.CoverImagePath = directory + $"{id}.{type}";

                    File.WriteAllBytes(directory + $"{id}.{type}", byteImage);
                }

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
                ImageType type;

                string directory = $"C:\\Users\\Oleg\\source\\repos\\VN_API\\Data\\VisualNovels\\{id}\\BackgroundImage\\";

                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return null;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    coverImage.CopyTo(ms);
                    var byteImage = ms.ToArray();

                    type = ImageFormat.GetImageFormat(byteImage);

                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    if (Directory.GetFiles(directory).Length > 0)
                    {
                        foreach (FileInfo file in directoryInfo.GetFiles())
                        {
                            file.Delete();
                        }
                    }

                    //vn.CoverImagePath = directory + $"{id}.{type}";

                    File.WriteAllBytes(directory + $"{id}.{type}", byteImage);
                }

                //_db.Entry(vn).State = EntityState.Modified;

                //await _db.SaveChangesAsync();

                return vn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> AddScreenshotsToVisualNovel(int id, List<IFormFile> screenshots)
        {
            try
            {
                string directory = $"C:\\Users\\Oleg\\source\\repos\\VN_API\\Data\\VisualNovels\\{id}\\Screenshots\\";

                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return null;
                }

                foreach (var iformfile in screenshots)
                {
                    ImageType type;

                    using (MemoryStream ms = new MemoryStream())
                    {
                        iformfile.CopyTo(ms);
                        var byteImage = ms.ToArray();

                        type = ImageFormat.GetImageFormat(byteImage);

                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        //if (Directory.GetFiles(directory).Length > 0)
                        //{
                        //    foreach (FileInfo file in directoryInfo.GetFiles())
                        //    {
                        //        file.Delete();
                        //    }
                        //}

                        //vn.CoverImagePath = directory + $"{id}.{type}";

                        File.WriteAllBytes(directory + $"{Guid.NewGuid()}.{type}", byteImage);
                    }
                }

                //_db.Entry(vn).State = EntityState.Modified;

                //await _db.SaveChangesAsync();

                return vn;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> UpdateVisualNovelAsync([FromBody] VisualNovel vn)
        {
            try
            {
                var visualNovel = _db.VisualNovels
                    .Include(dbvn => dbvn.Genres)
                    .Include(dbvn => dbvn.Tags)
                        .ThenInclude(tag => tag.Tag)
                    .Include(dbvn => dbvn.Platforms)
                    .Include(dbvn => dbvn.Languages)
                    .FirstOrDefault(dbvn => dbvn.Id == vn.Id);

                if (visualNovel == null)
                {
                    return null;
                }

                visualNovel.Title = vn.Title;
                visualNovel.VndbId = vn.VndbId;
                visualNovel.OriginalTitle = vn.OriginalTitle;
                visualNovel.Author = vn.Author;
                visualNovel.Status = vn.Status;
                visualNovel.ReadingTime = vn.ReadingTime;
                visualNovel.ReleaseYear = vn.ReleaseYear;
                visualNovel.AddedUserName = vn.AddedUserName;
                visualNovel.Description = vn.Description;
                visualNovel.Author = vn.Author;
                visualNovel.SteamLink = vn.SteamLink;
                visualNovel.Translator = vn.Translator;
                visualNovel.Links = vn.Links;
                //visualNovel.DateAdded = vn.DateAdded;
                visualNovel.DateUpdated = DateTime.Now;

                foreach (var tag in visualNovel.Tags)
                {
                    DeleteTagMetadataToVisualNovelAsync(tag.Tag.Id, visualNovel.Id);
                }

                foreach (var tag in vn.Tags)
                {
                    AddTagMetadataToVisualNovelAsync(tag.Tag.Id, visualNovel.Id, tag.SpoilerLevel);
                }

                //visualNovel.Tags = _db.TagsMetadata.Where(t => vn.Tags.Contains(t)).ToList();
                visualNovel.Genres = _db.Genres.Where(g => vn.Genres.Contains(g)).ToList();
                visualNovel.Platforms = _db.GamingPlatforms.Where(gp => vn.Platforms.Contains(gp)).ToList();
                visualNovel.Languages = _db.Languages.Where(l => vn.Languages.Contains(l)).ToList();

                _db.Entry(visualNovel).State = EntityState.Modified;

                await _db.SaveChangesAsync();

                return visualNovel;
            }
            catch (Exception ex)
            {
                return null;
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

        public async Task<List<Tag>> GetTagsAsync()
        {
            try
            {
                return await _db.Tags.ToListAsync();
            }
            catch (Exception ex)
            {
                return null;
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
        public async Task<List<TagMetadata>> GetVisualNovelTagsMetadataAsync(int visualNovelId)
        {
            try
            {
                return await _db.TagsMetadata.Where(tag => tag.VisualNovel.Id == visualNovelId).ToListAsync();
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
                return (await _db.Rating.Where(rating => rating.VisualNovelId == id).AverageAsync(rating => rating.Rating), await _db.Rating.Where(rating => rating.VisualNovelId == id).CountAsync());
            }
            catch (Exception ex)
            {
                return (-1, -1);
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
    }
}
