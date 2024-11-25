using System;
using System.Collections.Generic;
using CampaignManager.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CampaignManager.Service.Tests
{
    public static class TestDbInitializer
    {
        public static void Initialize(CampaignManagerDbContext context)
        {
            // Clear existing data
            context.Campaigns.RemoveRange(context.Campaigns);
            context.Users.RemoveRange(context.Users);
            context.Sessions.RemoveRange(context.Sessions);
            context.Notes.RemoveRange(context.Notes);
            context.NoteTypes.RemoveRange(context.NoteTypes);
            context.Participants.RemoveRange(context.Participants);
            context.Invitations.RemoveRange(context.Invitations);
            context.SaveChanges();

            // Add users
            var user1 = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "user1" };
            var user2 = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "user2" };
            var user3 = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "user3" };

            context.Users.AddRange(user1, user2, user3);

            // Add campaigns
            var campaign1 = new Campaign { Id = Guid.NewGuid(), Name = "Campaign 1", OwnerId = user1.Id };
            var campaign2 = new Campaign { Id = Guid.NewGuid(), Name = "Campaign 2", OwnerId = user2.Id };
            var campaign3 = new Campaign { Id = Guid.NewGuid(), Name = "Campaign 3", OwnerId = user3.Id };

            context.Campaigns.AddRange(campaign1, campaign2, campaign3);

            // Add campaign participants
            var participants = new List<CampaignParticipant>
            {
                new CampaignParticipant { Id = Guid.NewGuid(), ApplicationUserId = user2.Id, CampaignId = campaign1.Id, Role = Role.GameMaster },
                new CampaignParticipant { Id = Guid.NewGuid(), ApplicationUserId = user3.Id, CampaignId = campaign2.Id, Role = Role.Player },
                new CampaignParticipant { Id = Guid.NewGuid(), ApplicationUserId = user1.Id, CampaignId = campaign3.Id, Role = Role.PlayerAndGameMaster }
            };
            context.Participants.AddRange(participants);

            // Add invitations
            var invitations = new List<Invitation>
            {
                new Invitation { Id = 1, CampaignId = campaign1.Id, ApplicationUserId = user1.Id },
                new Invitation { Id = 2, CampaignId = campaign2.Id, ApplicationUserId = user2.Id },
                new Invitation { Id = 3, CampaignId = campaign3.Id, ApplicationUserId = user3.Id }
            };

            context.Invitations.AddRange(invitations);

            // Add sessions
            IList<Session> defaultSessions = new List<Session>
            {
                new Session { Id = Guid.NewGuid(), CampaignId = campaign1.Id, Name = "Session 1" },
                new Session { Id = Guid.NewGuid(), CampaignId = campaign2.Id, Name = "Session 2" },
                new Session { Id = Guid.NewGuid(), CampaignId = campaign3.Id, Name = "Session 3" }
            };

            // Add session players
            var sessionPlayers = new List<SessionPlayer>
            {
                new SessionPlayer { Id = Guid.NewGuid(), SessionId = defaultSessions[0].Id, ApplicationUserId = user1.Id },
                new SessionPlayer { Id = Guid.NewGuid(), SessionId = defaultSessions[1].Id, ApplicationUserId = user2.Id },
                new SessionPlayer { Id = Guid.NewGuid(), SessionId = defaultSessions[2].Id, ApplicationUserId = user3.Id }
            };

            context.SessionPlayers.AddRange(sessionPlayers);
            context.SaveChanges();

            context.Sessions.AddRange(defaultSessions);

            // Add templates
            var template1 = new Template { Id = Guid.NewGuid(), Name = "Template 1", OwnerId = user1.Id };
            var template2 = new Template { Id = Guid.NewGuid(), Name = "Template 2", OwnerId = user2.Id };
            var template3 = new Template { Id = Guid.NewGuid(), Name = "Template 3", OwnerId = user3.Id };

            context.Templates.AddRange(template1, template2, template3);

            // Add note types
            var noteType1 = new NoteType { Id = Guid.NewGuid(), Name = "Type 1", PlayerTemplateId = template1.Id, GameMasterTemplateId = template2.Id };
            var noteType2 = new NoteType { Id = Guid.NewGuid(), Name = "Type 2", PlayerTemplateId = template2.Id, GameMasterTemplateId = template3.Id };

            // Add generators
            var generator1 = new Generator { Id = Guid.NewGuid(), Name = "Generator 1", OwnerId = user1.Id, NoteTypeId = noteType1.Id };
            var generator2 = new Generator { Id = Guid.NewGuid(), Name = "Generator 2", OwnerId = user2.Id, NoteTypeId = noteType2.Id };
            var generator3 = new Generator { Id = Guid.NewGuid(), Name = "Generator 3", OwnerId = user3.Id, NoteTypeId = noteType1.Id };

            context.Generators.AddRange(generator1, generator2, generator3);

            context.NoteTypes.AddRange(noteType1, noteType2);

            // Add notes
            var note1 = new Note { Id = Guid.NewGuid(), SessionId = defaultSessions[0].Id, Content = "Note 1", NoteTypeId = noteType1.Id };
            var note2 = new Note { Id = Guid.NewGuid(), SessionId = defaultSessions[1].Id, Content = "Note 2", NoteTypeId = noteType2.Id };
            var note3 = new Note { Id = Guid.NewGuid(), SessionId = defaultSessions[2].Id, Content = "Note 3", NoteTypeId = noteType1.Id };

            context.Notes.AddRange(note1, note2, note3);

            // Add NoteLinks
            var noteLinks = new List<NoteLink>
            {
                new NoteLink { Id = Guid.NewGuid(), FromNoteId = note1.Id, ToNoteId = note2.Id, LinkType = "Reference" },
                new NoteLink { Id = Guid.NewGuid(), FromNoteId = note2.Id, ToNoteId = note3.Id, LinkType = "Related" }
            };

            context.NoteLinks.AddRange(noteLinks);

            context.SaveChanges();
        }
    }
}