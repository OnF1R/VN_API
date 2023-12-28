using VN_API.Models;
using VN_API.Models.Pagination;

namespace VN_API.Services.Interfaces
{
    public interface INovelService
    {
        // Visual Novel Service 
        Task<List<VisualNovel>> GetVisualNovelsAsync(PaginationParams @params);
        Task<VisualNovel> GetVisualNovelAsync(int id);
        Task<VisualNovel> AddVisualNovelAsync(VisualNovel visualNovel);
        Task<VisualNovel> AddCoverImageToVisualNovel(int id, IFormFile coverImage);
        Task<VisualNovel> UpdateVisualNovelAsync(VisualNovel visualNovel);
        Task<(bool, string)> DeleteVisualNovelAsync(VisualNovel visualNovel);
        Task<List<VisualNovel>> GetVisualNovelsWithTagAsync(int tagId);
        Task<List<VisualNovel>> GetVisualNovelsWithGenreAsync(int genreId);
        Task<List<VisualNovel>> GetVisualNovelsWithLanguageAsync(int languageId);
        Task<List<VisualNovel>> GetVisualNovelsWithGamingPlatformAsync(int gamingPlatformId);
        Task<List<VisualNovel>> SearchVisualNovel(string visualNovelTitleQuery);

        //Task<List<GamingPlatform>> GetVisualNovelGamingPlatformsAsync(int id);
        Task<List<Genre>> GetVisualNovelGenresAsync(int id);
        //Task<List<Tag>> GetVisualNovelTagsAsync(int id);
        //Task<List<Language>> GetVisualNovelLanguagesAsync(int id);

        // Gaming platform Service 
        Task<List<GamingPlatform>> GetGamingPlatformsAsync();
        Task<List<GamingPlatform>> GetGamingPlatformsWithVisualNovelsAsync();
        Task<GamingPlatform> GetGamingPlatformAsync(int id);
        Task<GamingPlatform> GetGamingPlatformWithVisualNovelsAsync(int id);
        Task<GamingPlatform> AddGamingPlatformAsync(string gamingPlatformName);
        void AddVisualNovelToGamingPlatformAsync(int gamingPlatformId, int vnId);
        void DeleteVisualNovelToGamingPlatformAsync(int gamingPlatformId, int vnId);
        Task<GamingPlatform> UpdateGamingPlatformAsync(int gamingPlatformId, string gamingPlatformName);
        Task<(bool, string)> DeleteGamingPlatformAsync(GamingPlatform gamingPlatform);

        // Genre Service 
        Task<List<Genre>> GetGenresAsync();
        Task<Genre> GetGenreAsync(int id);
        Task<Genre> AddGenreAsync(string genreName);
        void AddVisualNovelToGenreAsync(int genreId, int vnId);
        void DeleteVisualNovelToGenreAsync(int genreId, int vnId);
        Task<Genre> UpdateGenreAsync(int genreId, string genreName);
        Task<(bool, string)> DeleteGenreAsync(Genre genre);

        // Tag Novel Service 
        Task<List<Tag>> GetTagsAsync();
        Task<Tag> GetTagAsync(int id);
        Task<Tag> AddTagAsync(string tagName, string description);
        //void AddVisualNovelToTagAsync(int tagId, int vnId, SpoilerLevel spoilerLevel);
        //void DeleteVisualNovelToTagAsync(int tagId, int vnId);
        Task<Tag> UpdateTagAsync(int id, string tagName);
        Task<(bool, string)> DeleteTagAsync(Tag tag);

        // TagMetadata Novel Service 
        Task<TagMetadata> GetTagMetadata(Guid id);
        Task<List<TagMetadata>> GetTagMetadataAsync(int tagId);
        Task<List<TagMetadata>> GetVisualNovelTagsMetadataAsync(int visualNovelId);
        Task<TagMetadata> AddTagMetadataAsync(int tagId, int visualNovelId, SpoilerLevel spoilerLevel);
        Task<TagMetadata> UpdateTagMetadataAsync(Guid id, int tagId, int visualNovelId, SpoilerLevel spoilerLevel);
        void AddTagMetadataToVisualNovelAsync(int tagId, int vnId, SpoilerLevel spoilerLevel);
        void DeleteTagMetadataToVisualNovelAsync(int tagId, int vnId);
        Task<(bool, string)> DeleteTagMetadataAsync(TagMetadata tagMetadata);

        // Language Novel Service 
        Task<List<Language>> GetLanguagesAsync();
        Task<Language> GetLanguageAsync(int id);
        Task<Language> AddLanguageAsync(string languageName);
        void AddVisualNovelToLanguageAsync(int LanguageId, int vnId);
        void DeleteVisualNovelToLanguageAsync(int LanguageId, int vnId);
        Task<Language> UpdateLanguageAsync(int languageId, string languageName);
        Task<(bool, string)> DeleteLanguageAsync(Language language);
    }
}
