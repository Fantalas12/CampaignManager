using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Numerics;

namespace CampaignManager.Persistence.Models
{
    // This class is used to represent the database context.
    // The database context is responsible for connecting to the database, along with C# code and database tables for mapping to each other"
    public class CampaignManagerDbContext : IdentityDbContext<ApplicationUser>
    {

		public DbSet<Campaign> Campaigns { get; set; } = null!;
        public DbSet<CampaignParticipant> Participants { get; set; } = null!;
        public DbSet<Invitation> Invitations { get; set; } = null!;
		public DbSet<Session> Sessions { get; set; } = null!;
		public DbSet<SessionPlayer> SessionPlayers { get; set; } = null!;

        public DbSet<Note> Notes { get; set; } = null!;
        public DbSet<NoteType> NoteTypes { get; set; } = null!;
        public DbSet<Template> Templates { get; set; } = null!;
        public DbSet<NoteLink> NoteLinks { get; set; } = null!;
        public DbSet<Generator> Generators { get; set; } = null!;
        //public DbSet<NoteAccess> NoteAccesses { get; set; } = null!;
        //public DbSet<NoteAdmin> NoteAdmins { get; set; } = null!;
        public DbSet<NoteGenerator> NoteGenerators { get; set; } = null!;

        public CampaignManagerDbContext(DbContextOptions<CampaignManagerDbContext> options)
             : base(options)
        { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

            // CampaignParticipant to ApplicationUser and Campaign many-to-many relationship
            modelBuilder.Entity<CampaignParticipant>()
                .HasOne(cp => cp.ApplicationUser)
                .WithMany(u => u.CampaignParticipants)
                .HasForeignKey(cp => cp.ApplicationUserId);

			modelBuilder.Entity<CampaignParticipant>()
				.HasOne(p => p.Campaign)
				.WithMany(c => c.Participants)
				.HasForeignKey(p => p.CampaignId);

			// Campaign to ApplicationUser one-to-many relationship (ownership)
			modelBuilder.Entity<Campaign>()
				.HasOne(c => c.Owner)
				.WithMany(u => u.OwnedCampaigns)
				.HasForeignKey(c => c.OwnerId);

			//Casdade Delete for Participants on Campaign deletion
			modelBuilder.Entity<Campaign>()
                .HasMany(c => c.Participants)
                .WithOne(p => p.Campaign)
                .HasForeignKey(p => p.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

			// Cascade Delete for Invitations on Campaign deletion
			modelBuilder.Entity<Campaign>()
                .HasMany(c => c.Invitations)
                .WithOne(i => i.Campaign)
                .HasForeignKey(i => i.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

			//Cascade delete for Sessions on Campaign deletion
			modelBuilder.Entity<Campaign>()
                .HasMany(c => c.Sessions)
                .WithOne(s => s.Campaign)
                .HasForeignKey(s => s.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            //Cascade delete for SessionPlayers on Sesion deletion
			modelBuilder.Entity<Session>()
                .HasMany(s => s.SessionPlayers)
                .WithOne(sp => sp.Session)
                .HasForeignKey(sp => sp.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // SessionPlayer to ApplicationUser and Session many-to-many relationship
            modelBuilder.Entity<SessionPlayer>()
                .HasOne(sp => sp.ApplicationUser)
                .WithMany(u => u.SessionPlayers)
                .HasForeignKey(sp => sp.ApplicationUserId);

            modelBuilder.Entity<SessionPlayer>()
                .HasOne(sp => sp.Session)
                .WithMany(s => s.SessionPlayers)
                .HasForeignKey(sp => sp.SessionId);

            modelBuilder.Entity<Note>()
               .HasOne(n => n.NoteType)
               .WithMany(nt => nt.Notes)
               .HasForeignKey(n => n.NoteTypeId);

            /*
            modelBuilder.Entity<Note>()
                .HasMany(n => n.AccessControlList)
                .WithOne(ace => ace.Note)
                .HasForeignKey(ace => ace.NoteId)
                .OnDelete(DeleteBehavior.Cascade); */

            modelBuilder.Entity<NoteType>()
                .HasMany(nt => nt.Generators)
                .WithOne(g => g.NoteType)
                .HasForeignKey(g => g.NoteTypeId);

            modelBuilder.Entity<NoteLink>()
                .HasOne(nl => nl.ToNote)
                .WithMany(n => n.ToLinkedNotes)
                .HasForeignKey(nl => nl.ToNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteLink>()
                .HasOne(nl => nl.FromNote)
                .WithMany(n => n.FromLinkedNotes)
                .HasForeignKey(nl => nl.FromNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteType>()
               .HasOne(nt => nt.PlayerTemplate)
               .WithMany(t => t.PlayerNoteTypes)
               .HasForeignKey(nt => nt.PlayerTemplateId);

            modelBuilder.Entity<NoteType>()
             .HasOne(nt => nt.GameMasterTemplate)
             .WithMany(t => t.GameMasterNoteTypes)
             .HasForeignKey(nt => nt.GameMasterTemplateId);

            /*
            modelBuilder.Entity<NoteAdmin>()
                .HasOne(na => na.ApplicationUser)
                .WithMany(au => au.NoteAdmins)
                .HasForeignKey(na => na.ApplicationUserId);

            modelBuilder.Entity<NoteAdmin>()
                .HasOne(na => na.Campaign)
                .WithMany(c => c.NoteAdmins)
                .HasForeignKey(na => na.CampaignId)
                .OnDelete(DeleteBehavior.Cascade); */

            modelBuilder.Entity<Generator>()
                .HasOne(g => g.Owner)
                .WithMany(u => u.Generators)
                .HasForeignKey(g => g.OwnerId);

            modelBuilder.Entity<NoteType>()
                .HasOne(nt => nt.Owner)
                .WithMany(u => u.NoteTypes)
                .HasForeignKey(nt => nt.OwnerId);

            modelBuilder.Entity<Template>()
                .HasOne(t => t.Owner)
                .WithMany(u => u.Templates)
                .HasForeignKey(t => t.OwnerId);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Owner)
                .WithMany(u => u.Notes)
                .HasForeignKey(n => n.OwnerId);

            modelBuilder.Entity<Note>()
                .HasOne(n => n.Session)
                .WithMany(s => s.Notes)
                .HasForeignKey(n => n.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteGenerator>()
                .HasOne(ng => ng.Note)
                .WithMany(n => n.NoteGenerators)
                .HasForeignKey(ng => ng.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NoteGenerator>()
                .HasOne(ng => ng.Generator)
                .WithMany(g => g.NoteGenerators)
                .HasForeignKey(ng => ng.GeneratorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Note>(entity =>
            {
                entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v), // Convert ICollection<string> to a single string
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() // Convert string back to ICollection<string>
                );
            });

            //Value converter and comparer for the Tags property of the Note entity
            var stringListConverter = new ValueConverter<ICollection<string>, string>(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            var stringListComparer = new ValueComparer<ICollection<string>>(
                (c1, c2) => (c1 ?? new List<string>()).SequenceEqual(c2 ?? new List<string>()),
                c => c == null ? 0 : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c == null ? new List<string>() : c.ToList());

            modelBuilder.Entity<Note>(entity =>
            {
                entity.Property(e => e.Tags)
                    .HasConversion(stringListConverter)
                    .Metadata.SetValueComparer(stringListComparer);
            });



        }
    }
}
