using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using VN_API.Database;
using VN_API.Models;
using VN_API.Models.Pagination;
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
                    Id = vn.Id,
                    Title = vn.Title,
                    CoverImage = null,
                    OriginalTitle = vn.OriginalTitle,
                    ReadingTime = vn.ReadingTime,
                    Translator = vn.Translator,
                    Autor = vn.Autor,
                    ReleaseYear = vn.ReleaseYear,

                    DateAdded = DateTime.Now,
                    DateUpdated = DateTime.Now,

                    AddedUserName = vn.AddedUserName,
                    Description = vn.Description,

                    Platforms = null,
                    Tags = null,
                    Genres = null,
                    Languages = null,
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
                var vn = await _db.VisualNovels.FindAsync(id);

                if (vn == null)
                {
                    return null;
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    coverImage.CopyTo(ms);
                    var byteImage = ms.ToArray();

                    vn.CoverImage = byteImage;
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

        public async Task<VisualNovel> UpdateVisualNovelAsync([FromBody] VisualNovel vn)
        {
            try
            {
                var visualNovel = _db.VisualNovels
                    .Include(dbvn => dbvn.Genres)
                    .Include(dbvn => dbvn.Tags)
                    .Include(dbvn => dbvn.Platforms)
                    .Include(dbvn => dbvn.Languages)
                    .FirstOrDefault(dbvn => dbvn.Id == vn.Id);

                if (visualNovel == null)
                {
                    return null;
                }

                visualNovel.Title = vn.Title;
                visualNovel.OriginalTitle = vn.OriginalTitle;
                visualNovel.Autor = vn.Autor;
                visualNovel.ReadingTime = vn.ReadingTime;
                visualNovel.ReleaseYear = vn.ReleaseYear;
                visualNovel.AddedUserName = vn.AddedUserName;
                visualNovel.Description = vn.Description;
                visualNovel.DateAdded = vn.DateAdded;
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

                return vn;
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

                return await _db.VisualNovels.Where(vn => vn.Title.ToLower().Contains(query.Trim().ToLower())).ToListAsync();
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
    }
}
