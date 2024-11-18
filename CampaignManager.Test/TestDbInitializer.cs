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
            // Clear existing data
            context.Campaigns.RemoveRange(context.Campaigns);
            context.Users.RemoveRange(context.Users);
            context.Sessions.RemoveRange(context.Sessions);
            context.Notes.RemoveRange(context.Notes);
            context.NoteTypes.RemoveRange(context.NoteTypes);
            context.SaveChanges();

            // Add users
            var user1 = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "user1" };
            var user2 = new ApplicationUser { Id = Guid.NewGuid().ToString(), UserName = "user2" };

            context.Users.AddRange(user1, user2);

            // Add campaigns
            var campaign1 = new Campaign
            {
                Id = Guid.NewGuid(),
                Name = "Campaign 1",
                OwnerId = user1.Id
            };
            var campaign2 = new Campaign
            {
                Id = Guid.NewGuid(),
                Name = "Campaign 2",
                OwnerId = user2.Id
            };
            var campaign3 = new Campaign
            {
                Id = Guid.NewGuid(),
                Name = "Campaign 3",
                OwnerId = user1.Id
            };

            context.Campaigns.AddRange(campaign1, campaign2, campaign3);

            // Add sessions
            IList<Session> defaultSessions = new List<Session>
            {
                new Session
                {
                    Id = Guid.NewGuid(),
                    CampaignId = campaign1.Id,
                    Name = "Session 1"
                },
                new Session
                {
                    Id = Guid.NewGuid(),
                    CampaignId = campaign2.Id,
                    Name = "Session 2"
                },
                new Session
                {
                    Id = Guid.NewGuid(),
                    CampaignId = campaign3.Id,
                    Name = "Session 3"
                }
            };

            context.Sessions.AddRange(defaultSessions);

            // Add note types
            var noteType1 = new NoteType
            {
                Id = Guid.NewGuid(),
                Name = "Type 1"
            };
            var noteType2 = new NoteType
            {
                Id = Guid.NewGuid(),
                Name = "Type 2"
            };

            context.NoteTypes.AddRange(noteType1, noteType2);

            // Add notes
            IList<Note> defaultNotes = new List<Note>
            {
                new Note
                {
                    Id = Guid.NewGuid(),
                    SessionId = defaultSessions[0].Id,
                    Content = "Note 1",
                    NoteTypeId = noteType1.Id
                },
                new Note
                {
                    Id = Guid.NewGuid(),
                    SessionId = defaultSessions[1].Id,
                    Content = "Note 2",
                    NoteTypeId = noteType2.Id
                },
                new Note
                {
                    Id = Guid.NewGuid(),
                    SessionId = defaultSessions[2].Id,
                    Content = "Note 3",
                    NoteTypeId = noteType1.Id
                }
            };

            context.Notes.AddRange(defaultNotes);
            context.SaveChanges();
        }
    }
}

