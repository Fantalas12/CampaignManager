using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampaignManager.DTO;
using CampaignManager.Persistence.Models;
using CampaignManager.Persistence.Services;
using CampaignManager.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CampaignManager.Service.Tests
{
    public class CampaignManagerServiceTests
    {
        private readonly CampaignManagerDbContext _context;
        private readonly CampaignManagerService _service;

        public CampaignManagerServiceTests()
        {
            var options = new DbContextOptionsBuilder<CampaignManagerDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new CampaignManagerDbContext(options);
            TestDbInitializer.Initialize(_context);

            var mockLogger = new Mock<ILogger<CampaignManagerService>>();
            _service = new CampaignManagerService(_context, mockLogger.Object);
        }

        [Fact]
        public async Task GetCampaignById_ShouldReturnCampaign_WhenCampaignExists()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var campaignId = _context.Campaigns.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetCampaignById(campaignId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(campaignId, result.Id);
        }

        [Fact]
        public async Task AddCampaign_ShouldReturnTrue_WhenCampaignIsAdded()
        {
            // Arrange
            var campaign = new Campaign { Id = Guid.NewGuid(), Name = "New Campaign" };

            // Act
            var result = await _service.AddCampaign(campaign);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateCampaign_ShouldReturnTrue_WhenCampaignIsUpdated()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var campaign = _context.Campaigns.Skip(skipValue).First();
            campaign.Name = "Updated Campaign";

            // Act
            var result = await _service.UpdateCampaign(campaign);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteCampaignById_ShouldReturnTrue_WhenCampaignIsDeleted()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var campaign = _context.Campaigns.Skip(skipValue).First();

            // Act
            var result = await _service.DeleteCampaignById(campaign.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetOwnedCampaignsForUserById_ShouldReturnCampaigns_WhenUserOwnsCampaigns()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, _context.Users.Count());
            var userId = _context.Users.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetOwnedCampaignsForUserById(userId);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetCampaignsForUserById_ShouldReturnCampaigns_WhenUserIsParticipant()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var userId = _context.Users.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetCampaignsForUserById(userId);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task SaveCampaignToFile_ShouldReturnTrue_WhenCampaignIsSaved()
        {
            // Arrange
            var campaignDto = new CampaignDTO { Id = Guid.NewGuid(), Name = "Campaign DTO" };
            var filePath = "test_campaign.json";

            // Act
            var result = await _service.SaveCampaignToFile(campaignDto, filePath);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(filePath));

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async Task LoadCampaignFromFile_ShouldReturnCampaign_WhenFileExists()
        {
            // Arrange
            var campaignDto = new CampaignDTO { Id = Guid.NewGuid(), Name = "Campaign DTO" };
            var filePath = "test_campaign.json";
            var json = JsonConvert.SerializeObject(campaignDto);
            await File.WriteAllTextAsync(filePath, json);

            // Act
            var result = await _service.LoadCampaignFromFile(filePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(campaignDto.Id, result.Id);

            // Cleanup
            File.Delete(filePath);
        }

        [Fact]
        public async Task IsReservedCampaignNameForUser_ShouldReturnTrue_WhenNameIsReserved()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var campaign = _context.Campaigns.Skip(skipValue).First();
            var userId = campaign.OwnerId;

            // Act
            bool result = false;
            if (userId != null)
            {
                result = await _service.IsReservedCampaignNameForUser(campaign.Name, userId);
            }
            // Assert
            Assert.True(result);
        }


        [Fact]
        public async Task AddSession_ShouldReturnTrue_WhenSessionIsAdded()
        {
            var random = new Random();
            var skipValue = random.Next(0, 3);
            // Arrange
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Name = "New Session",
                CampaignId = _context.Campaigns.Skip(skipValue).First().Id,
                Date = DateTime.UtcNow
            };

            // Act
            var result = await _service.AddSession(session);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task UpdateSession_ShouldReturnTrue_WhenSessionIsUpdated()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var session = _context.Sessions.Skip(skipValue).First();
            session.Name = "Updated Session";

            // Act
            var result = await _service.UpdateSession(session);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteSessionById_ShouldReturnTrue_WhenSessionIsDeleted()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var session = _context.Sessions.Skip(skipValue).First();

            // Act
            var result = await _service.DeleteSessionById(session.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetSessionById_ShouldReturnSession_WhenSessionExists()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var sessionId = _context.Sessions.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetSessionById(sessionId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sessionId, result.Id);
        }

        [Fact]
        public async Task GetSessionsForCampaign_ShouldReturnSessions_WhenCampaignHasSessions()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var campaignId = _context.Campaigns.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetSessionsForCampaign(campaignId);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetPaginatedSessionsForCampaign_ShouldReturnPaginatedSessions_WhenCampaignHasSessions()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var campaignId = _context.Campaigns.Skip(skipValue).First().Id;
            int page = 1;
            int pageSize = 2;

            // Act
            var (sessions, totalCount) = await _service.GetPaginatedSessionsForCampaign(campaignId, page, pageSize);

            // Assert
            Assert.NotEmpty(sessions);
            Assert.True(totalCount > 0);
        }

        [Fact]
        public async Task IsReservedSessionNameForCampaign_ShouldReturnTrue_WhenNameIsReserved()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var session = _context.Sessions.Skip(skipValue).First();
            var campaignId = session.CampaignId;
            var name = session.Name;

            // Act
            var result = await _service.IsReservedSessionNameForCampaign(name, campaignId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsReservedSessionDateForCampaign_ShouldReturnTrue_WhenDateIsReserved()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var session = _context.Sessions.Skip(skipValue).First();
            var campaignId = session.CampaignId;
            var date = session.Date;

            // Act
            var result = await _service.IsReservedSessionDateForCampaign(date, campaignId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddNote_ShouldReturnTrue_WhenNoteIsAdded()
        {
            var random = new Random();
            var skipValue = random.Next(0, 3);
            // Arrange
            var note = new Note
            {
                Id = Guid.NewGuid(),
                SessionId = _context.Sessions.Skip(skipValue).First().Id,
                Content = "New Note",
                NoteTypeId = _context.NoteTypes.First().Id
            };

            // Act
            var result = await _service.AddNote(note);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetNoteById_ShouldReturnNote_WhenNoteExists()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var noteId = _context.Notes.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetNoteById(noteId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(noteId, result.Id);
        }

        [Fact]
        public async Task GetPaginatedNotesForSession_ShouldReturnPaginatedNotes_WhenSessionHasNotes()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var sessionId = _context.Sessions.Skip(skipValue).First().Id;
            int page = 1;
            int pageSize = 2;

            // Act
            var (notes, totalCount) = await _service.GetPaginatedNotesForSession(sessionId, page, pageSize);

            // Assert
            Assert.NotEmpty(notes);
            Assert.True(totalCount > 0);
        }

        [Fact]
        public async Task UpdateNote_ShouldReturnTrue_WhenNoteIsUpdated()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var note = _context.Notes.Skip(skipValue).First();
            note.Content = "Updated Note";

            // Act
            var result = await _service.UpdateNote(note);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteNoteById_ShouldReturnTrue_WhenNoteIsDeleted()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var note = _context.Notes.Skip(skipValue).First();

            // Act
            var result = await _service.DeleteNoteById(note.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddCampaignParticipant_ShouldAddParticipant()
        {
            // Arrange
            var random = new Random();
            var userId = _context.Users.Skip(random.Next(0, _context.Users.Count())).First().Id;
            var campaignId = _context.Campaigns.Skip(random.Next(0, _context.Campaigns.Count())).First().Id;
            var participant = new CampaignParticipant
            {
                Id = Guid.NewGuid(),
                ApplicationUserId = userId,
                CampaignId = campaignId,
                Role = Role.Player
            };

            // Act
            var result = await _service.AddCampaignParticipant(participant);

            // Assert
            Assert.True(result);
            Assert.Contains(_context.Participants, p => p.Id == participant.Id);
        }

        [Fact]
        public async Task UpdateCampaignParticipant_ShouldUpdateParticipant()
        {
            // Arrange
            var participant = _context.Participants.First();
            participant.Role = Role.GameMaster;

            // Act
            var result = await _service.UpdateCampaignParticipant(participant);

            // Assert
            Assert.True(result);
            Assert.Equal(Role.GameMaster, _context.Participants.First(p => p.Id == participant.Id).Role);
        }

        [Fact]
        public async Task DeleteCampaignParticipantById_ShouldDeleteParticipant()
        {
            // Arrange
            var participant = _context.Participants.First();

            // Act
            var result = await _service.DeleteCampaignParticipantById(participant.Id);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(_context.Participants, p => p.Id == participant.Id);
        }

        [Fact]
        public async Task GetPlayersForCampaign_ShouldReturnPlayers()
        {
            // Arrange
            var campaignId = _context.Campaigns.First().Id;

            // Act
            var players = await _service.GetPlayersForCampaign(campaignId);

            // Assert
            Assert.NotEmpty(players);
            Assert.All(players, p => Assert.True(p.Role == Role.Player || p.Role == Role.PlayerAndGameMaster));
        }

        [Fact]
        public async Task GetGMsForCampaign_ShouldReturnGMs()
        {
            // Arrange
            var campaignId = _context.Campaigns.First().Id;

            // Act
            var gms = await _service.GetGMsForCampaign(campaignId);

            // Assert
            Assert.NotEmpty(gms);
            Assert.All(gms, p => Assert.True(p.Role == Role.GameMaster || p.Role == Role.PlayerAndGameMaster));
        }

        [Fact]
        public async Task GetParticipantsForCampaign_ShouldReturnParticipants()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var campaignId = _context.Campaigns.Skip(skipValue).First().Id;

            // Act
            var participants = await _service.GetParticipantsForCampaign(campaignId);

            // Assert
            Assert.NotEmpty(participants);
        }

        [Fact]
        public async Task GetParticipantForCampaignByUserId_ShouldReturnParticipant()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var participant = _context.Participants.Skip(skipValue).First();

            // Act
            var result = await _service.GetParticipantForCampaignByUserId(participant.ApplicationUserId, participant.CampaignId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(participant.Id, result.Id);
        }

        [Fact]
        public async Task IsUserParticipant_ShouldReturnTrueIfUserIsParticipant()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var participant = _context.Participants.Skip(skipValue).First();

            // Act
            var result = await _service.IsUserParticipant(participant.CampaignId, participant.ApplicationUserId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddSessionPlayer_ShouldReturnTrue_WhenSessionPlayerIsAdded()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var sessionId = _context.Sessions.Skip(skipValue).First().Id;
            var userId = _context.Users.Skip(skipValue).First().Id;
            var sessionPlayer = new SessionPlayer
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                ApplicationUserId = userId
            };

            // Act
            var result = await _service.AddSessionPlayer(sessionPlayer);

            // Assert
            Assert.True(result);
            Assert.Contains(_context.SessionPlayers, sp => sp.Id == sessionPlayer.Id);
        }

        [Fact]
        public async Task RemoveSessionPlayer_ShouldReturnTrue_WhenSessionPlayerIsRemoved()
        {
            var random = new Random();
            var skipValue = random.Next(0, 3);
            // Arrange
            var sessionPlayer = new SessionPlayer
            {
                Id = Guid.NewGuid(),
                SessionId = _context.Sessions.Skip(skipValue).First().Id,
                ApplicationUserId = _context.Users.First().Id
            };
            _context.SessionPlayers.Add(sessionPlayer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.RemoveSessionPlayer(sessionPlayer);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(_context.SessionPlayers, sp => sp.Id == sessionPlayer.Id);
        }

        [Fact]
        public async Task AddInvitation_ShouldReturnTrue_WhenInvitationIsAdded()
        {
            var random = new Random();
            var skipValue = random.Next(0, 3);
            // Arrange
            var invitation = new Invitation
            {
                Id = 4,
                CampaignId = _context.Campaigns.Skip(skipValue).First().Id,
                ApplicationUserId = _context.Users.First().Id
            };

            // Act
            var result = await _service.AddInvitation(invitation);

            // Assert
            Assert.True(result);
            Assert.Contains(_context.Invitations, i => i.Id == invitation.Id);
        }

        [Fact]
        public async Task DeleteInvitationById_ShouldReturnTrue_WhenInvitationIsDeleted()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var invitation = _context.Invitations.Skip(skipValue).First();

            // Act
            var result = await _service.DeleteInvitationById(invitation.Id);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(_context.Invitations, i => i.Id == invitation.Id);
        }

        [Fact]
        public async Task DeleteOtherInvitationsForCampaign_ShouldReturnTrue_WhenInvitationsAreDeleted()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var campaignId = _context.Campaigns.Skip(skipValue).First().Id;
            var userId = _context.Users.First().Id;

            // Act
            var result = await _service.DeleteOtherInvitationsForCampaign(campaignId, userId);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(_context.Invitations, i => i.CampaignId == campaignId && i.ApplicationUserId == userId);
        }

        [Fact]
        public async Task GetInvitationsForUserById_ShouldReturnInvitations_WhenUserHasInvitations()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var userId = _context.Users.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetInvitationsForUserById(userId);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, i => Assert.Equal(userId, i.ApplicationUserId));
        }

        [Fact]
        public async Task GetInvitationById_ShouldReturnInvitation_WhenInvitationExists()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var invitationId = _context.Invitations.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetInvitationById(invitationId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(invitationId, result.Id);
        }

        [Fact]
        public async Task AddTemplate_ShouldReturnTrue_WhenTemplateIsAdded()
        {
            var template = new Template { Id = Guid.NewGuid(), Name = "New Template" };
            var result = await _service.AddTemplate(template);
            Assert.True(result);
        }

        [Fact]
        public async Task GetTemplateById_ShouldReturnTemplate_WhenTemplateExists()
        {
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var template = _context.Templates.Skip(skipValue).First();
            var result = await _service.GetTemplateById(template.Id);
            Assert.NotNull(result);
            Assert.Equal(template.Id, result.Id);
        }

        [Fact]
        public async Task GetPaginatedTemplates_ShouldReturnTemplates_WhenTemplatesExist()
        {
            var (templates, totalCount) = await _service.GetPaginatedTemplates(1, 10);
            Assert.NotEmpty(templates);
            Assert.Equal(_context.Templates.Count(), totalCount);
        }

        [Fact]
        public async Task UpdateTemplate_ShouldReturnTrue_WhenTemplateIsUpdated()
        {
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var template = _context.Templates.Skip(skipValue).First();
            template.Name = "Updated Template";
            var result = await _service.UpdateTemplate(template);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteTemplateById_ShouldReturnTrue_WhenTemplateIsDeleted()
        {
            var random = new Random();
            var skipValue = random.Next(0, 3);
            var template = _context.Templates.Skip(skipValue).First();
            var result = await _service.DeleteTemplateById(template.Id);
            Assert.True(result);
        }

        [Fact]
        public async Task GetGeneratorById_ShouldReturnGenerator_WhenGeneratorExists()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, _context.Generators.Count());
            var generatorId = _context.Generators.Skip(skipValue).First().Id;

            // Act
            var result = await _service.GetGeneratorById(generatorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(generatorId, result.Id);
        }

        [Fact]
        public async Task GetPaginatedGenerators_ShouldReturnPaginatedGenerators_WhenGeneratorsExist()
        {
            // Arrange
            int page = 1;
            int pageSize = 2;

            // Act
            var (generators, totalCount) = await _service.GetPaginatedGenerators(page, pageSize);

            // Assert
            Assert.NotEmpty(generators);
            Assert.True(totalCount > 0);
        }

        [Fact]
        public async Task GetPaginatedGeneratorsForNoteType_ShouldReturnPaginatedGenerators_WhenGeneratorsExist()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, _context.NoteTypes.Count());
            var noteTypeId = _context.NoteTypes.Skip(skipValue).First().Id;
            int page = 1;
            int pageSize = 2;

            // Act
            var generators = await _service.GetPaginatedGeneratorsForNoteType(noteTypeId, page, pageSize);

            // Assert
            Assert.NotEmpty(generators);
        }

        [Fact]
        public async Task UpdateGenerator_ShouldReturnTrue_WhenGeneratorIsUpdated()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, _context.Generators.Count());
            var generator = _context.Generators.Skip(skipValue).First();
            generator.Name = "Updated Generator";

            // Act
            var result = await _service.UpdateGenerator(generator);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteGeneratorById_ShouldReturnTrue_WhenGeneratorIsDeleted()
        {
            // Arrange
            var random = new Random();
            var skipValue = random.Next(0, _context.Generators.Count());
            var generator = _context.Generators.Skip(skipValue).First();

            // Act
            var result = await _service.DeleteGeneratorById(generator.Id);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(_context.Generators, g => g.Id == generator.Id);
        }

        [Fact]
        public async Task AddNoteLink_ShouldReturnTrue_WhenNoteLinkIsAdded()
        {
            // Arrange
            var noteLink = new NoteLink
            {
                Id = Guid.NewGuid(),
                FromNoteId = Guid.NewGuid(),
                ToNoteId = Guid.NewGuid(),
                LinkType = "New Link"
            };

            // Act
            var result = await _service.AddNoteLink(noteLink);

            // Assert
            Assert.True(result);
            Assert.NotNull(await _context.NoteLinks.FindAsync(noteLink.Id));
        }

        [Fact]
        public async Task GetNoteLinkById_ShouldReturnNoteLink_WhenNoteLinkExists()
        {
            // Arrange
            var random = new Random();
            var noteLink = _context.NoteLinks.Skip(random.Next(0, _context.NoteLinks.Count())).First();

            // Act
            var result = await _service.GetNoteLinkById(noteLink.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(noteLink.Id, result.Id);
        }

        [Fact]
        public async Task GetPaginatedToNoteLinksForNote_ShouldReturnPaginatedNoteLinks_WhenNoteLinksExist()
        {
            // Arrange
            var random = new Random();
            var fromNoteId = _context.NoteLinks.Skip(random.Next(0, _context.NoteLinks.Count())).First().FromNoteId;

            // Act
            var (result, totalCount) = await _service.GetPaginatedToNoteLinksForNote(fromNoteId, 1, 1);

            // Assert
            Assert.NotNull(result);
            Assert.True(totalCount > 0);
            Assert.Single(result);
        }

        [Fact]
        public async Task UpdateNoteLink_ShouldReturnTrue_WhenNoteLinkIsUpdated()
        {
            // Arrange
            var random = new Random();
            var noteLink = _context.NoteLinks.Skip(random.Next(0, _context.NoteLinks.Count())).First();
            noteLink.LinkType = "Updated";

            // Act
            var result = await _service.UpdateNoteLink(noteLink);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteNoteLinkById_ShouldReturnTrue_WhenNoteLinkIsDeleted()
        {
            // Arrange
            var random = new Random();
            var noteLink = _context.NoteLinks.Skip(random.Next(0, _context.NoteLinks.Count())).First();

            // Act
            var result = await _service.DeleteNoteLinkById(noteLink.Id);

            // Assert
            Assert.True(result);
            Assert.Null(await _context.NoteLinks.FindAsync(noteLink.Id));
        }
    

    
        [Fact]
        public async Task AddNoteGenerator_ShouldReturnTrue_WhenNoteGeneratorIsAdded()
        {
            // Arrange
            var random = new Random();
            var noteId = _context.Notes.Skip(random.Next(0, _context.Notes.Count())).First().Id;
            var generatorId = _context.Generators.Skip(random.Next(0, _context.Generators.Count())).First().Id;
            var noteGenerator = new NoteGenerator
            {
                Id = Guid.NewGuid(),
                NoteId = noteId,
                GeneratorId = generatorId,
                NextRunInGameDate = DateTime.Now.AddDays(1)
            };

            // Act
            var result = await _service.AddNoteGenerator(noteGenerator);

            // Assert
            Assert.True(result);
            Assert.Contains(_context.NoteGenerators, ng => ng.Id == noteGenerator.Id);
        }

        [Fact]
        public async Task GetNoteGeneratorById_ShouldReturnNoteGenerator_WhenNoteGeneratorExists()
        {
            // Arrange
            var random = new Random();
            var noteGeneratorId = _context.NoteGenerators.Skip(random.Next(0, _context.NoteGenerators.Count())).First().Id;

            // Act
            var result = await _service.GetNoteGeneratorById(noteGeneratorId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(noteGeneratorId, result.Id);
        }

        [Fact]
        public async Task GetPaginatedNoteGenerators_ShouldReturnPaginatedNoteGenerators_WhenNoteGeneratorsExist()
        {
            // Arrange
            int page = 1;
            int pageSize = 2;

            // Act
            var (noteGenerators, totalCount) = await _service.GetPaginatedNoteGenerators(page, pageSize);

            // Assert
            Assert.NotEmpty(noteGenerators);
            Assert.True(totalCount > 0);
        }

        [Fact]
        public async Task UpdateNoteGenerator_ShouldReturnTrue_WhenNoteGeneratorIsUpdated()
        {
            // Arrange
            var random = new Random();
            var noteGenerator = _context.NoteGenerators.Skip(random.Next(0, _context.NoteGenerators.Count())).First();
            noteGenerator.NextRunInGameDate = DateTime.Now.AddDays(2);

            // Act
            var result = await _service.UpdateNoteGenerator(noteGenerator);

            // Assert
            Assert.True(result);
            Assert.Equal(DateTime.Now.AddDays(2).Date, _context.NoteGenerators.First(ng => ng.Id == noteGenerator.Id).NextRunInGameDate?.Date);
        }

        [Fact]
        public async Task DeleteNoteGeneratorById_ShouldReturnTrue_WhenNoteGeneratorIsDeleted()
        {
            // Arrange
            var random = new Random();
            var noteGenerator = _context.NoteGenerators.Skip(random.Next(0, _context.NoteGenerators.Count())).First();

            // Act
            var result = await _service.DeleteNoteGeneratorById(noteGenerator.Id);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(_context.NoteGenerators, ng => ng.Id == noteGenerator.Id);
        }

        [Fact]
        public async Task AddNoteType_ShouldReturnTrue_WhenNoteTypeIsAdded()
        {
            // Arrange
            var noteType = new NoteType { Id = Guid.NewGuid(), Name = "New NoteType" };

            // Act
            var result = await _service.AddNoteType(noteType);

            // Assert
            Assert.True(result);
            Assert.Contains(_context.NoteTypes, nt => nt.Id == noteType.Id);
        }

        [Fact]
        public async Task GetNoteTypeById_ShouldReturnNoteType_WhenNoteTypeExists()
        {
            // Arrange
            var noteType = _context.NoteTypes.First();

            // Act
            var result = await _service.GetNoteTypeById(noteType.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(noteType.Id, result.Id);
        }

        [Fact]
        public async Task GetPaginatedNoteTypes_ShouldReturnPaginatedNoteTypes_WhenNoteTypesExist()
        {
            // Arrange
            int page = 1;
            int pageSize = 2;

            // Act
            var (noteTypes, totalCount) = await _service.GetPaginatedNoteTypes(page, pageSize);

            // Assert
            Assert.NotEmpty(noteTypes);
            Assert.True(totalCount > 0);
        }

        [Fact]
        public async Task UpdateNoteType_ShouldReturnTrue_WhenNoteTypeIsUpdated()
        {
            // Arrange
            var noteType = _context.NoteTypes.First();
            noteType.Name = "Updated NoteType";

            // Act
            var result = await _service.UpdateNoteType(noteType);

            // Assert
            Assert.True(result);
            Assert.Equal("Updated NoteType", _context.NoteTypes.First(nt => nt.Id == noteType.Id).Name);
        }

        [Fact]
        public async Task DeleteNoteTypeById_ShouldReturnTrue_WhenNoteTypeIsDeleted()
        {
            // Arrange
            var noteType = _context.NoteTypes.First();

            // Act
            var result = await _service.DeleteNoteTypeById(noteType.Id);

            // Assert
            Assert.True(result);
            Assert.DoesNotContain(_context.NoteTypes, nt => nt.Id == noteType.Id);
        }



    }
}
