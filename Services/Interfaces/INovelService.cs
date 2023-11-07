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

        // Gaming platform Service 
        Task<List<GamingPlatform>> GetGamingPlatformsAsync();
        Task<GamingPlatform> GetGamingPlatformAsync(Guid id);
        Task<GamingPlatform> AddGamingPlatformAsync(GamingPlatform gamingPlatform);
        Task<GamingPlatform> UpdateGamingPlatformAsync(GamingPlatform gamingPlatform);
        Task<(bool, string)> DeleteGamingPlatformAsync(GamingPlatform gamingPlatform);

        // Genre Service 
        Task<List<Genre>> GetGenresAsync();
        Task<Genre> GetGenreAsync(Guid id);
        Task<Genre> AddGenreAsync(Genre genre);
        Task<Genre> UpdateGenreAsync(Genre genre);
        Task<(bool, string)> DeleteGenreAsync(Genre genre);

        // Tag Novel Service 
        Task<List<Tag>> GetTagsAsync();
        Task<Tag> GetTagAsync(Guid id);
        Task<Tag> AddTagAsync(Tag tag);
        Task<Tag> UpdateTagAsync(Tag tag);
        Task<(bool, string)> DeleteTagAsync(Tag tag);

        // Language Novel Service 
        Task<List<Language>> GetLanguagesAsync();
        Task<Language> GetLanguageAsync(Guid id);
        Task<Language> AddLanguageAsync(Language language);
        Task<Language> UpdateLanguageAsync(Language language);
        Task<(bool, string)> DeleteLanguageAsync(Language language);
    }
}
