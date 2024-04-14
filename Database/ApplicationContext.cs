using Microsoft.EntityFrameworkCore;
using VN_API.Models;

namespace VN_API.Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<VisualNovel> VisualNovels { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagMetadata> TagsMetadata { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<GamingPlatform> GamingPlatforms { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Translator> Translators { get; set; }
        public DbSet<VisualNovelRating> Rating { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;User Id=postgres;Password=moloko990;Database=vndb;");
            //optionsBuilder.UseSqlite("Data Source=vn.db");
            //optionsBuilder.UseLazyLoadingProxies();
        }
    }
}
