using VN_API.Models;
using VN_API.Models.Pagination;

namespace VN_API.Services.Interfaces
{
    public interface INovelService
    {
        // Visual Novel Service 
        /// <summary>
        /// Get Visual Novel From Database using pagination options
        /// </summary>
        /// <param name="params">PaginationParams Model</param>
        /// <returns>List<VisualNovel> if there are one or more matches, otherwise null</returns>
        Task<List<VisualNovel>> GetVisualNovelsAsync(PaginationParams @params);
        /// <summary>
        /// Get Visual Novel From Database by identifier with tags Spoiler Level
        /// </summary>
        /// <param name="id">The Visual Novel ID corresponding to the record in the database</param>
        /// <param name="spoilerLevel">The Spoiler Level enum value</param>
        /// <returns>VisualNovel if a match was found, otherwise null</returns>
        Task<VisualNovel> GetVisualNovelAsync(int id, SpoilerLevel spoilerLevel = SpoilerLevel.None);
        /// <summary>
        /// Add Visual Novel To Database
        /// </summary>
        /// <param name="visualNovel">VisualNovel Model</param>
        /// <returns>Visual Novel if Visual Novel was added to database, otherwise null</returns>
        Task<VisualNovel> AddVisualNovelAsync(VisualNovel visualNovel);
        /// <summary>
        /// Add byte[] to Visual Novel CoverImage Column in Database
        /// </summary>
        /// <param name="id">The Visual Novel ID corresponding to the record in the database</param>
        /// <param name="coverImage">Already verified image file (png, jpg, jpeg, bmp, tiff, gif)</param>
        /// <returns>Visual Novel if Cover Image was added to Visual Novel, otherwise null</returns>
        Task<VisualNovel> AddCoverImageToVisualNovel(int id, IFormFile coverImage);
        /// <summary>
        /// Update Visual Novel Record in Database
        /// </summary>
        /// <param name="visualNovel">VisualNovel model, where Id has a match in the database</param>
        /// <returns>Visual Novel if Visual Novel was updated, otherwise null</returns>
        Task<VisualNovel> UpdateVisualNovelAsync(VisualNovel visualNovel);
        /// <summary>
        /// Delete Visual Novel Record From Database
        /// </summary>
        /// <param name="visualNovel">VisualNovel model, where Id has a match in the database</param>
        /// <returns>True if Visual Novel was removed and successful message, otherwise false and error message</returns>
        Task<(bool, string)> DeleteVisualNovelAsync(VisualNovel visualNovel);
        /// <summary>
        /// Get Visual Novels From Database With Tag and None Spoiler Level 
        /// </summary>
        /// <param name="tagId">The Tag ID corresponding to the record in the database</param>
        /// <returns>List VisualNovel if there are one or more matches, otherwise null</returns>
        Task<List<VisualNovel>> GetVisualNovelsWithTagAsync(int tagId);
        /// <summary>
        /// Get Visual Novel From Database With Tag and Spoiler Level
        /// </summary>
        /// <param name="tagId">The Tag ID corresponding to the record in the database</param>
        /// <param name="spoilerLevel">The Spoiler Level enum value</param>
        /// <returns>List VisualNovel if there are one or more matches, otherwise null</returns>
        Task<List<VisualNovel>> GetVisualNovelsWithTagAndSpoilerLevelAsync(int tagId, SpoilerLevel spoilerLevel);
        /// <summary>
        /// Get Visual Novels From Database With Genre 
        /// </summary>
        /// <param name="genreId">The Genre ID corresponding to the record in the database</param>
        /// <returns>List VisualNovel if there are one or more matches, otherwise null</returns>
        Task<List<VisualNovel>> GetVisualNovelsWithGenreAsync(int genreId);
        /// <summary>
        /// Get Visual Novels From Database With Language
        /// </summary>
        /// <param name="languageId">The Language ID corresponding to the record in the database</param>
        /// <returns>List VisualNovel if there are one or more matches, otherwise null</returns>
        Task<List<VisualNovel>> GetVisualNovelsWithLanguageAsync(int languageId);
        /// <summary>
        /// Get Visual Novel From Database With Gaming Platform
        /// </summary>
        /// <param name="gamingPlatformId">The Gaming Platform ID corresponding to the record in the database</param>
        /// <returns>List VisualNovel if there are one or more matches, otherwise null</returns>
        Task<List<VisualNovel>> GetVisualNovelsWithGamingPlatformAsync(int gamingPlatformId);
        /// <summary>
        /// Search Visual Novels in Database based on a match in the title
        /// </summary>
        /// <param name="visualNovelTitleQuery">Suggested title of the Visual Novel or part thereof</param>
        /// <returns>List VisualNovel if there are one or more matches, otherwise null</returns>
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
