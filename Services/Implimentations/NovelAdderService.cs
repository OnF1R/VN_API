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
                // Without Lazy Loading The Same Result

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
                    AddVisualNovelToGamingPlatformAsync(gp.Id, visualNovel.Id);

                foreach (var t in tags)
                    AddVisualNovelToTagAsync(t.Id, visualNovel.Id);

                foreach (var l in languages)
                    AddVisualNovelToLanguageAsync(l.Id, visualNovel.Id); ;

                foreach (var g in genres)
                    AddVisualNovelToGenreAsync(g.Id, visualNovel.Id);

                await _db.SaveChangesAsync();

                return await _db.VisualNovels.FindAsync(vn.Id);
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
                visualNovel.Rating = vn.Rating;
                visualNovel.ReadingTime = vn.ReadingTime;
                visualNovel.ReleaseYear = vn.ReleaseYear;
                visualNovel.AddedUserName = vn.AddedUserName;
                visualNovel.Description = vn.Description;
                visualNovel.DateAdded = vn.DateAdded;
                visualNovel.DateUpdated = DateTime.Now;


                visualNovel.Genres = _db.Genres.Where(g => vn.Genres.Contains(g)).ToList();
                visualNovel.Tags = _db.Tags.Where(t => vn.Tags.Contains(t)).ToList();
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

        public async Task<GamingPlatform> AddGamingPlatformAsync(string gamingPlatformName)
        {
            try
            {
                var gamingPlatform = new GamingPlatform()
                {
                    Id = Guid.NewGuid(),
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

        public void AddVisualNovelToGamingPlatformAsync(Guid gpId, Guid vnId)
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

        public void DeleteVisualNovelToGamingPlatformAsync(Guid gpId, Guid vnId)
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

        public async Task<GamingPlatform> UpdateGamingPlatformAsync(Guid gamingPlatformId, string gamingPlatformName)
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

        public async Task<Language> AddLanguageAsync(string languageName)
        {
            try
            {
                var language = new Language()
                {
                    Id = Guid.NewGuid(),
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

        public void AddVisualNovelToLanguageAsync(Guid languageId, Guid vnId)
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

        public void DeleteVisualNovelToLanguageAsync(Guid languageId, Guid vnId)
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

        public async Task<Language> UpdateLanguageAsync(Guid languageId, string languageName)
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

        public async Task<Genre> AddGenreAsync(string genreName)
        {
            try
            {
                var dbGenre = new Genre
                {
                    Id = Guid.NewGuid(),
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

        public void AddVisualNovelToGenreAsync(Guid genreId, Guid vnId)
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

        public void DeleteVisualNovelToGenreAsync(Guid genreId, Guid vnId)
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

        public async Task<Genre> UpdateGenreAsync(Guid genreId, string genreName)
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

        public async Task<Tag> AddTagAsync(string tagName)
        {
            try
            {
                var tag = new Tag()
                {
                    Id = Guid.NewGuid(),
                    Name = tagName,
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

        public void AddVisualNovelToTagAsync(Guid tagId, Guid vnId)
        {
            try
            {
                var tag = _db.Tags.Find(tagId);
                var vn = _db.VisualNovels.Find(vnId);


                tag.VisualNovels.Add(vn);
                //await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }

        public void DeleteVisualNovelToTagAsync(Guid tagId, Guid vnId)
        {
            try
            {
                var tag = _db.Tags.Find(tagId);
                var vn = _db.VisualNovels.Find(vnId);

                tag.VisualNovels.Remove(vn);
                //await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<Tag> UpdateTagAsync(Guid tagId, string tagName)
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
    }
}
