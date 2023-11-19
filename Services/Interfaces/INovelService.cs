using VN_API.Models;

namespace VN_API.Services.Interfaces
{
    public interface INovelService
    {
        // Visual Novel Service 
        Task<List<VisualNovel>> GetVisualNovelsAsync();
        Task<VisualNovel> GetVisualNovelAsync(Guid id);
        Task<VisualNovel> AddVisualNovelAsync(VisualNovel visualNovel);
        Task<VisualNovel> UpdateVisualNovelAsync(VisualNovel visualNovel);
        Task<(bool, string)> DeleteVisualNovelAsync(VisualNovel visualNovel);

        //Task<List<GamingPlatform>> GetVisualNovelGamingPlatformsAsync(Guid id);
        Task<List<Genre>> GetVisualNovelGenresAsync(Guid id);
        //Task<List<Tag>> GetVisualNovelTagsAsync(Guid id);
        //Task<List<Language>> GetVisualNovelLanguagesAsync(Guid id);

        // Gaming platform Service 
        Task<List<GamingPlatform>> GetGamingPlatformsAsync();
        Task<GamingPlatform> GetGamingPlatformAsync(Guid id);
        Task<GamingPlatform> AddGamingPlatformAsync(string gamingPlatformName);
        void AddVisualNovelToGamingPlatformAsync(Guid gamingPlatformId, Guid vnId);
        void DeleteVisualNovelToGamingPlatformAsync(Guid gamingPlatformId, Guid vnId);
        Task<GamingPlatform> UpdateGamingPlatformAsync(Guid gamingPlatformId, string gamingPlatformName);
        Task<(bool, string)> DeleteGamingPlatformAsync(GamingPlatform gamingPlatform);

        // Genre Service 
        Task<List<Genre>> GetGenresAsync();
        Task<Genre> GetGenreAsync(Guid id);
        Task<Genre> AddGenreAsync(string genreName);
        void AddVisualNovelToGenreAsync(Guid genreId, Guid vnId);
        void DeleteVisualNovelToGenreAsync(Guid genreId, Guid vnId);
        Task<Genre> UpdateGenreAsync(Guid genreId, string genreName);
        Task<(bool, string)> DeleteGenreAsync(Genre genre);

        // Tag Novel Service 
        Task<List<Tag>> GetTagsAsync();
        Task<Tag> GetTagAsync(Guid id);
        Task<Tag> AddTagAsync(string tagName);
        void AddVisualNovelToTagAsync(Guid tagId, Guid vnId);
        void DeleteVisualNovelToTagAsync(Guid tagId, Guid vnId);
        Task<Tag> UpdateTagAsync(Guid id, string tagName);
        Task<(bool, string)> DeleteTagAsync(Tag tag);

        // Language Novel Service 
        Task<List<Language>> GetLanguagesAsync();
        Task<Language> GetLanguageAsync(Guid id);
        Task<Language> AddLanguageAsync(string languageName);
        void AddVisualNovelToLanguageAsync(Guid LanguageId, Guid vnId);
        void DeleteVisualNovelToLanguageAsync(Guid LanguageId, Guid vnId);
        Task<Language> UpdateLanguageAsync(Guid languageId, string languageName);
        Task<(bool, string)> DeleteLanguageAsync(Language language);
    }
}
