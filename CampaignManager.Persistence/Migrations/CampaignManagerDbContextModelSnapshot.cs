﻿// <auto-generated />
using System;
using CampaignManager.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CampaignManager.Persistence.Migrations
{
    [DbContext(typeof(CampaignManagerDbContext))]
    partial class CampaignManagerDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("CampaignManager.Persistence.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers", (string)null);
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Campaign", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Edited")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("GameTime")
                        .HasColumnType("TEXT");

                    b.Property<byte[]>("Image")
                        .HasColumnType("BLOB");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OwnerId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Campaigns");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.CampaignParticipant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApplicationUserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CampaignId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Role")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.HasIndex("CampaignId");

                    b.ToTable("Participants");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Generator", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsBuiltIn")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("NoteId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("NoteTypeId")
                        .HasColumnType("TEXT");

                    b.Property<string>("OwnerId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Script")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NoteId");

                    b.HasIndex("NoteTypeId");

                    b.HasIndex("OwnerId");

                    b.ToTable("Generators");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Invitation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApplicationUserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CampaignId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Role")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.HasIndex("CampaignId");

                    b.ToTable("Invitations");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Note", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("InGameDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ModifiedDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("NoteTypeId")
                        .HasColumnType("TEXT");

                    b.Property<string>("OwnerId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("SessionId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NoteTypeId");

                    b.HasIndex("OwnerId");

                    b.HasIndex("SessionId");

                    b.ToTable("Notes");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.NoteGenerator", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("GeneratorId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("NextRunInGameDate")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("NoteId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GeneratorId");

                    b.HasIndex("NoteId");

                    b.ToTable("NoteGenerators");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.NoteLink", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("FromNoteId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LinkType")
                        .HasColumnType("TEXT");

                    b.Property<Guid>("ToNoteId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("FromNoteId");

                    b.HasIndex("ToNoteId");

                    b.ToTable("NoteLinks");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.NoteType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("GameMasterTemplateId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OwnerId")
                        .HasColumnType("TEXT");

                    b.Property<Guid?>("PlayerTemplateId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("GameMasterTemplateId");

                    b.HasIndex("OwnerId");

                    b.HasIndex("PlayerTemplateId");

                    b.ToTable("NoteTypes");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Session", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("CampaignId")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameMasterId")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CampaignId");

                    b.HasIndex("GameMasterId");

                    b.ToTable("Sessions");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.SessionPlayer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ApplicationUserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("SessionId")
                        .HasColumnType("TEXT");

                    b.Property<string>("SessonPlayerRole")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ApplicationUserId");

                    b.HasIndex("SessionId");

                    b.ToTable("SessionPlayers");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Template", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("Content")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("OwnerId")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Templates");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Campaign", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "Owner")
                        .WithMany("OwnedCampaigns")
                        .HasForeignKey("OwnerId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.CampaignParticipant", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "ApplicationUser")
                        .WithMany("CampaignParticipants")
                        .HasForeignKey("ApplicationUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CampaignManager.Persistence.Models.Campaign", "Campaign")
                        .WithMany("Participants")
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ApplicationUser");

                    b.Navigation("Campaign");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Generator", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.Note", "Note")
                        .WithMany()
                        .HasForeignKey("NoteId");

                    b.HasOne("CampaignManager.Persistence.Models.NoteType", "NoteType")
                        .WithMany("Generators")
                        .HasForeignKey("NoteTypeId");

                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "Owner")
                        .WithMany("Generators")
                        .HasForeignKey("OwnerId");

                    b.Navigation("Note");

                    b.Navigation("NoteType");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Invitation", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CampaignManager.Persistence.Models.Campaign", "Campaign")
                        .WithMany("Invitations")
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ApplicationUser");

                    b.Navigation("Campaign");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Note", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.NoteType", "NoteType")
                        .WithMany("Notes")
                        .HasForeignKey("NoteTypeId");

                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "Owner")
                        .WithMany("Notes")
                        .HasForeignKey("OwnerId");

                    b.HasOne("CampaignManager.Persistence.Models.Session", "Session")
                        .WithMany("Notes")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("NoteType");

                    b.Navigation("Owner");

                    b.Navigation("Session");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.NoteGenerator", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.Generator", "Generator")
                        .WithMany("NoteGenerators")
                        .HasForeignKey("GeneratorId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("CampaignManager.Persistence.Models.Note", "Note")
                        .WithMany("NoteGenerators")
                        .HasForeignKey("NoteId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Generator");

                    b.Navigation("Note");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.NoteLink", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.Note", "FromNote")
                        .WithMany("FromLinkedNotes")
                        .HasForeignKey("FromNoteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CampaignManager.Persistence.Models.Note", "ToNote")
                        .WithMany("ToLinkedNotes")
                        .HasForeignKey("ToNoteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("FromNote");

                    b.Navigation("ToNote");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.NoteType", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.Template", "GameMasterTemplate")
                        .WithMany("GameMasterNoteTypes")
                        .HasForeignKey("GameMasterTemplateId");

                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "Owner")
                        .WithMany("NoteTypes")
                        .HasForeignKey("OwnerId");

                    b.HasOne("CampaignManager.Persistence.Models.Template", "PlayerTemplate")
                        .WithMany("PlayerNoteTypes")
                        .HasForeignKey("PlayerTemplateId");

                    b.Navigation("GameMasterTemplate");

                    b.Navigation("Owner");

                    b.Navigation("PlayerTemplate");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Session", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.Campaign", "Campaign")
                        .WithMany("Sessions")
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "GameMaster")
                        .WithMany("OwnedSessions")
                        .HasForeignKey("GameMasterId");

                    b.Navigation("Campaign");

                    b.Navigation("GameMaster");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.SessionPlayer", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "ApplicationUser")
                        .WithMany("SessionPlayers")
                        .HasForeignKey("ApplicationUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CampaignManager.Persistence.Models.Session", "Session")
                        .WithMany("SessionPlayers")
                        .HasForeignKey("SessionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ApplicationUser");

                    b.Navigation("Session");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Template", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", "Owner")
                        .WithMany("Templates")
                        .HasForeignKey("OwnerId");

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("CampaignManager.Persistence.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.ApplicationUser", b =>
                {
                    b.Navigation("CampaignParticipants");

                    b.Navigation("Generators");

                    b.Navigation("NoteTypes");

                    b.Navigation("Notes");

                    b.Navigation("OwnedCampaigns");

                    b.Navigation("OwnedSessions");

                    b.Navigation("SessionPlayers");

                    b.Navigation("Templates");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Campaign", b =>
                {
                    b.Navigation("Invitations");

                    b.Navigation("Participants");

                    b.Navigation("Sessions");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Generator", b =>
                {
                    b.Navigation("NoteGenerators");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Note", b =>
                {
                    b.Navigation("FromLinkedNotes");

                    b.Navigation("NoteGenerators");

                    b.Navigation("ToLinkedNotes");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.NoteType", b =>
                {
                    b.Navigation("Generators");

                    b.Navigation("Notes");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Session", b =>
                {
                    b.Navigation("Notes");

                    b.Navigation("SessionPlayers");
                });

            modelBuilder.Entity("CampaignManager.Persistence.Models.Template", b =>
                {
                    b.Navigation("GameMasterNoteTypes");

                    b.Navigation("PlayerNoteTypes");
                });
#pragma warning restore 612, 618
        }
    }
}
