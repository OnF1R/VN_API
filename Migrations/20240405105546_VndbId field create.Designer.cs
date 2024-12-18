﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using VN_API.Database;

#nullable disable

namespace VNAPI.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20240405105546_VndbId field create")]
    partial class VndbIdfieldcreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GamingPlatformVisualNovel", b =>
                {
                    b.Property<int>("PlatformsId")
                        .HasColumnType("integer");

                    b.Property<int>("VisualNovelsId")
                        .HasColumnType("integer");

                    b.HasKey("PlatformsId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("GamingPlatformVisualNovel");
                });

            modelBuilder.Entity("GenreVisualNovel", b =>
                {
                    b.Property<int>("GenresId")
                        .HasColumnType("integer");

                    b.Property<int>("VisualNovelsId")
                        .HasColumnType("integer");

                    b.HasKey("GenresId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("GenreVisualNovel");
                });

            modelBuilder.Entity("LanguageVisualNovel", b =>
                {
                    b.Property<int>("LanguagesId")
                        .HasColumnType("integer");

                    b.Property<int>("VisualNovelsId")
                        .HasColumnType("integer");

                    b.HasKey("LanguagesId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("LanguageVisualNovel");
                });

            modelBuilder.Entity("VN_API.Models.Author", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Source")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("VN_API.Models.GamingPlatform", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("GamingPlatforms");
                });

            modelBuilder.Entity("VN_API.Models.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("VN_API.Models.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("VN_API.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("VN_API.Models.TagMetadata", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("SpoilerLevel")
                        .HasColumnType("integer");

                    b.Property<int>("TagId")
                        .HasColumnType("integer");

                    b.Property<int?>("VisualNovelId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TagId");

                    b.HasIndex("VisualNovelId");

                    b.ToTable("TagsMetadata");
                });

            modelBuilder.Entity("VN_API.Models.Translator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Source")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Translators");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<Guid>("AdddeUserId")
                        .HasColumnType("uuid");

                    b.Property<string>("AddedUserName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("AuthorId")
                        .HasColumnType("integer");

                    b.Property<string>("Autor")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CoverImagePath")
                        .HasColumnType("text");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("DateUpdated")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("OriginalTitle")
                        .HasColumnType("text");

                    b.Property<int>("ReadingTime")
                        .HasColumnType("integer");

                    b.Property<int>("ReleaseYear")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Translator")
                        .HasColumnType("text");

                    b.Property<int?>("TranslatorId")
                        .HasColumnType("integer");

                    b.Property<string>("VndbId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("TranslatorId");

                    b.ToTable("VisualNovels");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovelRating", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("AddingTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("Rating")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.Property<int>("VisualNovelId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Rating");
                });

            modelBuilder.Entity("GamingPlatformVisualNovel", b =>
                {
                    b.HasOne("VN_API.Models.GamingPlatform", null)
                        .WithMany()
                        .HasForeignKey("PlatformsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VN_API.Models.VisualNovel", null)
                        .WithMany()
                        .HasForeignKey("VisualNovelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GenreVisualNovel", b =>
                {
                    b.HasOne("VN_API.Models.Genre", null)
                        .WithMany()
                        .HasForeignKey("GenresId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VN_API.Models.VisualNovel", null)
                        .WithMany()
                        .HasForeignKey("VisualNovelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("LanguageVisualNovel", b =>
                {
                    b.HasOne("VN_API.Models.Language", null)
                        .WithMany()
                        .HasForeignKey("LanguagesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VN_API.Models.VisualNovel", null)
                        .WithMany()
                        .HasForeignKey("VisualNovelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VN_API.Models.TagMetadata", b =>
                {
                    b.HasOne("VN_API.Models.Tag", "Tag")
                        .WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VN_API.Models.VisualNovel", "VisualNovel")
                        .WithMany("Tags")
                        .HasForeignKey("VisualNovelId");

                    b.Navigation("Tag");

                    b.Navigation("VisualNovel");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovel", b =>
                {
                    b.HasOne("VN_API.Models.Author", null)
                        .WithMany("VisualNovels")
                        .HasForeignKey("AuthorId");

                    b.HasOne("VN_API.Models.Translator", null)
                        .WithMany("VisualNovels")
                        .HasForeignKey("TranslatorId");
                });

            modelBuilder.Entity("VN_API.Models.Author", b =>
                {
                    b.Navigation("VisualNovels");
                });

            modelBuilder.Entity("VN_API.Models.Translator", b =>
                {
                    b.Navigation("VisualNovels");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovel", b =>
                {
                    b.Navigation("Tags");
                });
#pragma warning restore 612, 618
        }
    }
}
