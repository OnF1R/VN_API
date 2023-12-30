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
    [Migration("20231127173640_CoverImageColumnInVisualNovelAdd")]
    partial class CoverImageColumnInVisualNovelAdd
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0");

            modelBuilder.Entity("GamingPlatformVisualNovel", b =>
                {
                    b.Property<Guid>("PlatformsId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("VisualNovelsId")
                        .HasColumnType("TEXT");

                    b.HasKey("PlatformsId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("GamingPlatformVisualNovel");
                });

            modelBuilder.Entity("GenreVisualNovel", b =>
                {
                    b.Property<Guid>("GenresId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("VisualNovelsId")
                        .HasColumnType("TEXT");

                    b.HasKey("GenresId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("GenreVisualNovel");
                });

            modelBuilder.Entity("LanguageVisualNovel", b =>
                {
                    b.Property<Guid>("LanguagesId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("VisualNovelsId")
                        .HasColumnType("TEXT");

                    b.HasKey("LanguagesId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("LanguageVisualNovel");
                });

            modelBuilder.Entity("TagVisualNovel", b =>
                {
                    b.Property<Guid>("TagsId")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("VisualNovelsId")
                        .HasColumnType("TEXT");

                    b.HasKey("TagsId", "VisualNovelsId");

                    b.HasIndex("VisualNovelsId");

                    b.ToTable("TagVisualNovel");
                });

            modelBuilder.Entity("VN_API.Models.GamingPlatform", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("GamingPlatforms");
                });

            modelBuilder.Entity("VN_API.Models.Genre", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Genres");
                });

            modelBuilder.Entity("VN_API.Models.Language", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("VN_API.Models.Tag", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("VN_API.Models.VisualNovel", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("AddedUserName")
                        .IsRequired()
                        .HasColumnType("TEXT");

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

                    b.Property<float>("Rating")
                        .HasColumnType("REAL");

                    b.Property<int>("ReadingTime")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReleaseYear")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Translator")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

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

            modelBuilder.Entity("TagVisualNovel", b =>
                {
                    b.HasOne("VN_API.Models.Tag", null)
                        .WithMany()
                        .HasForeignKey("TagsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("VN_API.Models.VisualNovel", null)
                        .WithMany()
                        .HasForeignKey("VisualNovelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}