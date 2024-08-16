﻿// <auto-generated />
using System;
using System.Collections.Generic;
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
    [Migration("20240629064301_VisualNovelLists")]
    partial class VisualNovelLists
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AuthorVisualNovel", b =>
                {
                    b.Property<int>("AuthorId")
                        .HasColumnType("integer");

                    b.Property<int>("VisualNovelsId")
                        .HasColumnType("integer");

                    b.HasKey("AuthorId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("AuthorVisualNovel");
                });

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

                    b.Property<string>("VndbId")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("VN_API.Models.DownloadLink", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("GamingPlatformId")
                        .HasColumnType("integer");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("VisualNovelId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("GamingPlatformId");

                    b.HasIndex("VisualNovelId");

                    b.ToTable("DownloadLinks");
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

            modelBuilder.Entity("VN_API.Models.OtherLink", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("VisualNovelId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("VisualNovelId");

                    b.ToTable("OtherLinks");
                });

            modelBuilder.Entity("VN_API.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool?>("Applicable")
                        .HasColumnType("boolean");

                    b.Property<int?>("Category")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<string>("EnglishName")
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("VndbId")
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

                    b.Property<string>("BackgroundImageFileName")
                        .HasColumnType("text");

                    b.Property<string>("CoverImageFileName")
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

                    b.Property<int?>("ReleaseYear")
                        .HasColumnType("integer");

                    b.Property<List<string>>("ScreenshotFileNames")
                        .IsRequired()
                        .HasColumnType("text[]");

                    b.Property<int>("Status")
                        .HasColumnType("integer");

                    b.Property<string>("SteamLink")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("TranslateLinkForSteam")
                        .HasColumnType("text");

                    b.Property<int?>("TranslatorId")
                        .HasColumnType("integer");

                    b.Property<string>("VndbId")
                        .HasColumnType("text");

                    b.Property<int?>("VndbLengthInMinutes")
                        .HasColumnType("integer");

                    b.Property<double?>("VndbRating")
                        .HasColumnType("double precision");

                    b.Property<int?>("VndbVoteCount")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TranslatorId");

                    b.ToTable("VisualNovels");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovelList", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsCustom")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsPrivate")
                        .HasColumnType("boolean");

                    b.Property<int>("ListTypeId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("ListTypeId");

                    b.ToTable("VisualNovelLists");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovelListEntry", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("AddingTime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("VisualNovelId")
                        .HasColumnType("integer");

                    b.Property<int>("VisualNovelListId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("VisualNovelId");

                    b.HasIndex("VisualNovelListId");

                    b.ToTable("VisualNovelListEntries");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovelListType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("ListTypes");
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

            modelBuilder.Entity("AuthorVisualNovel", b =>
                {
                    b.HasOne("VN_API.Models.Author", null)
                        .WithMany()
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VN_API.Models.VisualNovel", null)
                        .WithMany()
                        .HasForeignKey("VisualNovelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
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

            modelBuilder.Entity("VN_API.Models.DownloadLink", b =>
                {
                    b.HasOne("VN_API.Models.GamingPlatform", "GamingPlatform")
                        .WithMany()
                        .HasForeignKey("GamingPlatformId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VN_API.Models.VisualNovel", "VisualNovel")
                        .WithMany("Links")
                        .HasForeignKey("VisualNovelId");

                    b.Navigation("GamingPlatform");

                    b.Navigation("VisualNovel");
                });

            modelBuilder.Entity("VN_API.Models.OtherLink", b =>
                {
                    b.HasOne("VN_API.Models.VisualNovel", "VisualNovel")
                        .WithMany("OtherLinks")
                        .HasForeignKey("VisualNovelId");

                    b.Navigation("VisualNovel");
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
                    b.HasOne("VN_API.Models.Translator", "Translator")
                        .WithMany("VisualNovels")
                        .HasForeignKey("TranslatorId");

                    b.Navigation("Translator");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovelList", b =>
                {
                    b.HasOne("VN_API.Models.VisualNovelListType", "ListType")
                        .WithMany()
                        .HasForeignKey("ListTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ListType");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovelListEntry", b =>
                {
                    b.HasOne("VN_API.Models.VisualNovel", "VisualNovel")
                        .WithMany()
                        .HasForeignKey("VisualNovelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VN_API.Models.VisualNovelList", "VisualNovelList")
                        .WithMany("VisualNovelListEntries")
                        .HasForeignKey("VisualNovelListId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("VisualNovel");

                    b.Navigation("VisualNovelList");
                });

            modelBuilder.Entity("VN_API.Models.Translator", b =>
                {
                    b.Navigation("VisualNovels");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovel", b =>
                {
                    b.Navigation("Links");

                    b.Navigation("OtherLinks");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovelList", b =>
                {
                    b.Navigation("VisualNovelListEntries");
                });
#pragma warning restore 612, 618
        }
    }
}