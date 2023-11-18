using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VN_API.Database;
using VN_API.Models;
using VN_API.Services.Interfaces;

namespace VN_API.Services
{
    public class NovelAdderService : INovelService
    {
        private readonly ApplicationContext _db;

        public NovelAdderService(ApplicationContext db)
        {
            _db = db;
        }

        #region Visual Novel

        public async Task<List<VisualNovel>> GetVisualNovelsAsync()
        {
            try
            {
                //var visualNovels = _db.VisualNovels
                //    .Include(vn => vn.Genres)
                //    .Include(vn => vn.Tags)
                //    .Include(vn => vn.Platforms)
                //    .Include(vn => vn.Languages)
                //    .ToListAsync();

                var visualNovels = _db.VisualNovels.ToListAsync();

                return await visualNovels;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> GetVisualNovelAsync(Guid id)
        {
            try
            {
                return await _db.VisualNovels.FindAsync(id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> AddVisualNovelAsync([FromBody] VisualNovel vn)
        {
            try
            {
                var gamingPlatforms = _db.GamingPlatforms.Where(gp => vn.Platforms.Contains(gp)).ToList();
                var tags = _db.Tags.Where(tag => vn.Tags.Contains(tag)).ToList();
                var genres = _db.Genres.Where(genre => vn.Genres.Contains(genre)).ToList();
                var languages = _db.Languages.Where(lang => vn.Languages.Contains(lang)).ToList();

                var visualNovel = new VisualNovel()
                {
                    Id = vn.Id,
                    Title = vn.Title,
                    OriginalTitle = vn.OriginalTitle,
                    Rating = vn.Rating,
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

                foreach (var gp in gamingPlatforms)
                    await AddVisualNovelToGamingPlatformAsync(gp.Id, visualNovel.Id);

                foreach (var t in tags)
                    await AddVisualNovelToTagAsync(t.Id, visualNovel.Id);

                foreach (var l in languages)
                    await AddVisualNovelToLanguageAsync(l.Id, visualNovel.Id); ;

                foreach (var g in genres)
                    await AddVisualNovelToGenreAsync(g.Id, visualNovel.Id);

                await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vn.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> UpdateVisualNovelAsync(VisualNovel vn)
        {
            try
            {
                _db.Entry(vn).State = EntityState.Modified;
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

        public async Task<List<Genre>> GetVisualNovelGenresAsync(Guid id)
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

        public async Task<GamingPlatform> GetGamingPlatformAsync(Guid id)
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

        public async Task<GamingPlatform> AddGamingPlatformAsync(GamingPlatform gamingPlatform)
        {
            try
            {
                await _db.GamingPlatforms.AddAsync(gamingPlatform);
                await _db.SaveChangesAsync();
                return await _db.GamingPlatforms.FindAsync(gamingPlatform.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> AddVisualNovelToGamingPlatformAsync(Guid gpId, Guid vnId)
        {
            try
            {
                var gamingPlatform = await _db.GamingPlatforms.FindAsync(gpId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                if (gamingPlatform == null || vn == null)
                {
                    return null;
                }

                gamingPlatform.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vnId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> DeleteVisualNovelToGamingPlatformAsync(Guid gpId, Guid vnId)
        {
            try
            {
                var gamingPlatform = await _db.GamingPlatforms.FindAsync(gpId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                if (gamingPlatform == null || vn == null)
                {
                    return null;
                }

                gamingPlatform.VisualNovels.Remove(vn);
                await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vnId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<GamingPlatform> UpdateGamingPlatformAsync(GamingPlatform gamingPlatform)
        {
            try
            {
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

        public async Task<Language> GetLanguageAsync(Guid id)
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

        public async Task<Language> AddLanguageAsync(Language language)
        {
            try
            {
                await _db.Languages.AddAsync(language);
                await _db.SaveChangesAsync();
                return await _db.Languages.FindAsync(language.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> AddVisualNovelToLanguageAsync(Guid languageId, Guid vnId)
        {
            try
            {
                var language = await _db.Languages.FindAsync(languageId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                if (language == null || vn == null)
                {
                    return null;
                }

                language.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vnId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> DeleteVisualNovelToLanguageAsync(Guid languageId, Guid vnId)
        {
            try
            {
                var language = await _db.Languages.FindAsync(languageId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                if (language == null || vn == null)
                {
                    return null;
                }

                language.VisualNovels.Remove(vn);
                await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vnId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Language> UpdateLanguageAsync(Language language)
        {
            try
            {
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
                var dbVn = await _db.GamingPlatforms.FindAsync(language.Id);

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

        public async Task<Genre> GetGenreAsync(Guid id)
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

        public async Task<Genre> AddGenreAsync([FromBody] Genre genre)
        {
            try
            {
                var dbGenre = new Genre
                {
                    //Id = Guid.NewGuid(),
                    Id = genre.Id,
                    Name = genre.Name,
                    // Другие свойства, если есть
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

        public async Task<VisualNovel> AddVisualNovelToGenreAsync(Guid genreId, Guid vnId)
        {
            try
            {
                var genre = await _db.Genres.FindAsync(genreId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                if (genre == null || vn == null)
                {
                    return null;
                }

                genre.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vnId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> DeleteVisualNovelToGenreAsync(Guid genreId, Guid vnId)
        {
            try
            {
                var genre = await _db.Genres.FindAsync(genreId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                if (genre == null || vn == null)
                {
                    return null;
                }

                genre.VisualNovels.Remove(vn);
                await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vnId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Genre> UpdateGenreAsync(Genre genre)
        {
            try
            {
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

        public async Task<Tag> GetTagAsync(Guid id)
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

        public async Task<Tag> AddTagAsync(Tag tag)
        {
            try
            {
                await _db.Tags.AddAsync(tag);
                await _db.SaveChangesAsync();
                return await _db.Tags.FindAsync(tag.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> AddVisualNovelToTagAsync(Guid tagId, Guid vnId)
        {
            try
            {
                var tag = await _db.Tags.FindAsync(tagId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                if (tag == null || vn == null)
                {
                    return null;
                }

                tag.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vnId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<VisualNovel> DeleteVisualNovelToTagAsync(Guid tagId, Guid vnId)
        {
            try
            {
                var tag = await _db.Tags.FindAsync(tagId);
                var vn = await _db.VisualNovels.FindAsync(vnId);

                if (tag == null || vn == null)
                {
                    return null;
                }

                tag.VisualNovels.Remove(vn);
                await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vnId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            try
            {
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
    }
}
