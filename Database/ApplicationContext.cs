using Microsoft.EntityFrameworkCore;
using VN_API.Models;

namespace VN_API.Database
{
    public class ApplicationContext : DbContext
    {
        public DbSet<VisualNovel> VisualNovels { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<GamingPlatform> GamingPlatforms { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
