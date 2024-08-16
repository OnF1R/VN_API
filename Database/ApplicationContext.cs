using Microsoft.EntityFrameworkCore;
using VN_API.Models;
using VN_API.Models.Comment;

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
        public DbSet<DownloadLink> DownloadLinks { get; set; }
        public DbSet<OtherLink> OtherLinks { get; set; }
        public DbSet<RelatedAnimeLink> AnimeLinks { get; set; }
        public DbSet<VisualNovelListType> ListTypes { get; set; }
        public DbSet<VisualNovelList> VisualNovelLists { get; set; }
        public DbSet<VisualNovelListEntry> VisualNovelListEntries { get; set; }
        public DbSet<VisualNovelComment> Comments { get; set; }
        public DbSet<VisualNovelCommentRating> CommentRatings { get; set; }
        public DbSet<RelatedNovel> RelatedNovels { get; set; }
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            //Database.EnsureDeleted();
            //Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.UseNpgsql("Server=localhost;Port=5432;User Id=postgres;Password=moloko990;Database=vndb;");
            //optionsBuilder.UseSqlite("Data Source=vn.db");
            //optionsBuilder.UseLazyLoadingProxies();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RelatedNovel>()
            .HasKey(vr => new { vr.VisualNovelId, vr.RelatedVisualNovelId });

            modelBuilder.Entity<RelatedNovel>()
                .HasOne(vr => vr.VisualNovel)
                .WithMany(v => v.RelatedNovels)
                .HasForeignKey(vr => vr.VisualNovelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RelatedNovel>()
                .HasOne(vr => vr.RelatedVisualNovel)
                .WithMany()
                .HasForeignKey(vr => vr.RelatedVisualNovelId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VisualNovel>()
                .HasIndex(vn => vn.Id)
                .IsUnique();

            modelBuilder.Entity<VisualNovel>()
                .HasIndex(vn => vn.Title);

            modelBuilder.Entity<VisualNovel>()
                .HasIndex(vn => vn.LinkName)
                .IsUnique();

            modelBuilder.Entity<Tag>()
                .HasIndex(tag => tag.Id)
                .IsUnique();

            modelBuilder.Entity<Author>()
                .HasIndex(author => author.Id)
                .IsUnique();

            modelBuilder.Entity<TagMetadata>()
                .HasIndex("VisualNovelId");

            modelBuilder.Entity<VisualNovelRating>()
                .HasIndex(vnRating => vnRating.Id)
                .IsUnique();

            modelBuilder.Entity<VisualNovelRating>()
                .HasIndex(vnRating => vnRating.UserId);

            modelBuilder.Entity<VisualNovelRating>()
                .HasIndex(vnRating => vnRating.VisualNovelId);

            modelBuilder.Entity<DownloadLink>()
                .HasIndex(downloadLink => downloadLink.Id)
                .IsUnique();

            modelBuilder.Entity<DownloadLink>()
                .HasIndex("VisualNovelId");

            modelBuilder.Entity<OtherLink>()
                .HasIndex(otherLink => otherLink.Id)
                .IsUnique();

            modelBuilder.Entity<OtherLink>()
                .HasIndex("VisualNovelId");

            modelBuilder.Entity<RelatedAnimeLink>()
                .HasIndex(animeLink => animeLink.Id)
                .IsUnique();

            modelBuilder.Entity<RelatedAnimeLink>()
                .HasIndex("VisualNovelId");

            modelBuilder.Entity<VisualNovelList>()
                .HasIndex(vnList => vnList.Id)
                .IsUnique();

            modelBuilder.Entity<VisualNovelList>()
                .HasIndex(vnList => vnList.UserId);

            modelBuilder.Entity<VisualNovelListEntry>()
                .HasIndex(vnListEntry => vnListEntry.Id)
                .IsUnique();

            modelBuilder.Entity<VisualNovelComment>()
                .HasIndex(comment => comment.Id)
                .IsUnique();

            modelBuilder.Entity<VisualNovelComment>()
                .HasIndex(comment => comment.UserId);

            modelBuilder.Entity<VisualNovelComment>()
                .HasIndex(comment => comment.VisualNovelId);

            modelBuilder.Entity<VisualNovelCommentRating>()
                .HasIndex(comment => comment.Id)
                .IsUnique();

            modelBuilder.Entity<VisualNovelCommentRating>()
                .HasIndex(comment => comment.UserId);

            modelBuilder.Entity<VisualNovelCommentRating>()
                .HasIndex(comment => comment.CommentId);
        }
    }
}
