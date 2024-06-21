using System;
using System.Net;
using Amazon.S3;
using Amazon.S3.Transfer;
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
        private IAmazonS3 _s3Client;
        private const string bucketName = "astral-novel";

        public NovelAdderService(ApplicationContext db, IMemoryCache cache, IAmazonS3 s3Client)
        {
            _db = db;
            _cache = cache;
            _s3Client = s3Client;
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

            VisualNovel visualNovel = await _db.VisualNovels.FirstAsync(vn => vn.Id == id);

            if (visualNovel.VndbId != null)
            {
                var vndbResult = await vndbService.GetVisualNovelTags(visualNovel.VndbId.ToString());

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
                    .Include(vn => vn.Links)
                    .Include(vn => vn.OtherLinks)
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

                    //CoverImagePath = null, TODO

                    OriginalTitle = vn.OriginalTitle,
                    ReadingTime = vn.ReadingTime,
                    Status = vn.Status,
                    SteamLink = vn.SteamLink,
                    TranslateLinkForSteam = vn.TranslateLinkForSteam,
                    
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
                    //Links = vn.Links,
                };

                await _db.VisualNovels.AddAsync(visualNovel);

                await _db.SaveChangesAsync();

                // TODO Not Find Visual Novel

                foreach (var gp in gamingPlatforms)
                    AddVisualNovelToGamingPlatformAsync(gp.Id, visualNovel.Id);

                //foreach (var t in tags)
                //    AddTagMetadataToVisualNovelAsync(t.Tag.Id, visualNovel.Id, t.SpoilerLevel);

                foreach (var l in languages)
                    AddVisualNovelToLanguageAsync(l.Id, visualNovel.Id); ;

                foreach (var g in genres)
                    AddVisualNovelToGenreAsync(g.Id, visualNovel.Id);

                foreach (var a in vn.Author)
                    await AddVisualNovelToAuthorAsync(a.Id, visualNovel.Id);
                
                if (vn.Translator != null)
                    await AddVisualNovelToTranslatorAsync(vn.Translator.Id, visualNovel.Id);

                if (vn.Links != null)
                {
                    foreach (var link in vn.Links)
                    {
                        await AddDownloadLinkAsync(link);
                        await AddDownloadLinksToVisualNovelAsync(link.Id, visualNovel.Id);
                    }
                }     

                if (vn.OtherLinks != null)
                {
                    foreach (var link in vn.OtherLinks)
                    {
                        await AddOtherLinkAsync(link);
                        await AddOtherLinksToVisualNovelAsync(link.Id, visualNovel.Id);
                    }
                        
                }
                    
                await _db.SaveChangesAsync();

                await LoadOrUpdateVNDBRating(visualNovel.Id);

                if (visualNovel.VndbId != null)
                    await UpdateVisualNovelTagsFromVNDB(visualNovel.Id);

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
                int indexLastDot = coverImage.FileName.LastIndexOf('.');

                int fileExtensionsNameLength = coverImage.FileName.Length - indexLastDot;

                string fileExtensionName = coverImage.FileName.Substring(indexLastDot + 1, fileExtensionsNameLength - 1);

                string pathToLoad = $"{id}/CoverImage";

                string fileName = $"{id}.{fileExtensionName}";

                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return null;
                }

                //TODO Send file to S3

                await LoadToS3(coverImage, pathToLoad, fileName);

                vn.CoverImageFileName = fileName;

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

                string pathToLoad = $"{id}/BackgroundImage";

                string fileName = $"{id}.{fileExtensionName}";

                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return null;
                }

                await LoadToS3(coverImage, pathToLoad, fileName);

                vn.BackgroundImageFileName = fileName;

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

                    string pathToLoad = $"{id}/Screenshots";

                    string fileName = $"{Guid.NewGuid()}.{fileExtensionName}";

                    await LoadToS3(iformfile, pathToLoad, fileName);

                    vn.ScreenshotFileNames.Add(fileName);

                    _db.Entry(vn).State = EntityState.Modified;

                    await _db.SaveChangesAsync();
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

        public async Task LoadToS3(IFormFile file, string path, string fileName)
        {
            var fileTransferUtility = new TransferUtility(_s3Client);

            using (var newMemoryStream = new MemoryStream())
            {
                await file.CopyToAsync(newMemoryStream);

                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = newMemoryStream,
                    Key = $"{path}/{fileName}",
                    BucketName = bucketName,
                    ContentType = file.ContentType,
                    CannedACL = S3CannedACL.PublicRead // Доступ к файлу публичный
                };

                await fileTransferUtility.UploadAsync(uploadRequest);
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
                    .Include(dbvn => dbvn.Translator)
                    .Include(dbvn => dbvn.Author)
                    .Include(dbvn => dbvn.Links)
                    .Include(dbvn => dbvn.OtherLinks)
                    .FirstOrDefault(dbvn => dbvn.Id == vn.Id);

                if (visualNovel == null)
                {
                    return null;
                }

                visualNovel.Title = vn.Title;
                visualNovel.VndbId = vn.VndbId;
                visualNovel.OriginalTitle = vn.OriginalTitle;
                visualNovel.Status = vn.Status;
                visualNovel.ReadingTime = vn.ReadingTime;
                visualNovel.ReleaseYear = vn.ReleaseYear;
                visualNovel.AddedUserName = vn.AddedUserName;
                visualNovel.Description = vn.Description;
                
                visualNovel.SteamLink = vn.SteamLink;
                visualNovel.TranslateLinkForSteam = vn.TranslateLinkForSteam;
                //visualNovel.Translator = vn.Translator;
                //visualNovel.Links = vn.Links;
                //visualNovel.DateAdded = vn.DateAdded;
                visualNovel.DateUpdated = DateTime.Now;

                foreach (var tag in visualNovel.Tags)
                    DeleteTagMetadataToVisualNovelAsync(tag.Tag.Id, visualNovel.Id);

                foreach (var tag in vn.Tags)
                    AddTagMetadataToVisualNovelAsync(tag.Tag.Id, visualNovel.Id, tag.SpoilerLevel);

                //visualNovel.Tags = _db.TagsMetadata.Where(t => vn.Tags.Contains(t)).ToList();
                visualNovel.Genres = _db.Genres.Where(g => vn.Genres.Contains(g)).ToList();
                visualNovel.Platforms = _db.GamingPlatforms.Where(gp => vn.Platforms.Contains(gp)).ToList();
                visualNovel.Languages = _db.Languages.Where(l => vn.Languages.Contains(l)).ToList();
                visualNovel.Author = _db.Authors.Where(a => vn.Author.Contains(a)).ToList();
                if (vn.Translator is not null)
                    visualNovel.Translator = await _db.Translators.FindAsync(vn.Translator.Id);

                List<DownloadLink> tempDownloadLinks = new List<DownloadLink>();

                tempDownloadLinks.AddRange(visualNovel.Links);

                //TODO Maybe change??
                foreach (var downloadLink in tempDownloadLinks)
                {
                    await DeleteDownloadLinksToVisualNovelAsync(downloadLink.Id, visualNovel.Id);
                    await DeleteDownloadLink(downloadLink);
                }

                foreach (var downloadLink in vn.Links)
                {
                    var link = await AddDownloadLinkAsync(downloadLink);
                    await AddDownloadLinksToVisualNovelAsync(link.Id, visualNovel.Id);
                }

                List<OtherLink> tempOtherLinks = new List<OtherLink>();

                tempOtherLinks.AddRange(visualNovel.OtherLinks);

                foreach (var otherLink in tempOtherLinks)
                {
                    await DeleteOtherLinksToVisualNovelAsync(otherLink.Id, visualNovel.Id);
                    await DeleteOtherLink(otherLink);
                }

                foreach (var otherLink in vn.OtherLinks)
                {
                    var link = await AddOtherLinkAsync(otherLink);
                    await AddOtherLinksToVisualNovelAsync(link.Id, visualNovel.Id);
                }

                await LoadOrUpdateVNDBRating(visualNovel.Id);

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

                vn.Links.Add(downloadLink);
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

                vn.Links.Remove(downloadLink);
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

        
    }
}
        #endregion