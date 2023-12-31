﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VN_API.Database;

#nullable disable

namespace VNAPI.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20231224205208_UpdateTagsAndPrimaryKeys")]
    partial class UpdateTagsAndPrimaryKeys
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0");

            modelBuilder.Entity("GamingPlatformVisualNovel", b =>
                {
                    b.Property<int>("PlatformsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VisualNovelsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("PlatformsId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("GamingPlatformVisualNovel");
                });

            modelBuilder.Entity("GenreVisualNovel", b =>
                {
                    b.Property<int>("GenresId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VisualNovelsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("GenresId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("GenreVisualNovel");
                });

            modelBuilder.Entity("LanguageVisualNovel", b =>
                {
                    b.Property<int>("LanguagesId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VisualNovelsId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LanguagesId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("LanguageVisualNovel");
                });

            modelBuilder.Entity("VN_API.Models.Author", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Authors");
                });

            modelBuilder.Entity("VN_API.Models.GamingPlatform", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("GamingPlatforms");
                });

            modelBuilder.Entity("VN_API.Models.Genre", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("VN_API.Models.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("VN_API.Models.Tag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("VN_API.Models.TagMetadata", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<int>("SpoilerLevel")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TagId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VisualNovelId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TagId");

                    b.HasIndex("VisualNovelId");

                    b.ToTable("TagsMetadata");
                });

            modelBuilder.Entity("VN_API.Models.Translator", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Source")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Translators");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("AdddeUserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("AddedUserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int?>("AuthorId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Autor")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("CoverImage")
                        .HasColumnType("BLOB");

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("DateUpdated")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OriginalTitle")
                        .HasColumnType("TEXT");

                    b.Property<int>("ReadingTime")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReleaseYear")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Translator")
                        .HasColumnType("TEXT");

                    b.Property<int?>("TranslatorId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("TranslatorId");

                    b.ToTable("VisualNovels");
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
                        .HasForeignKey("VisualNovelId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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
