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
            var campaignId = _context.Campaigns.First().Id;

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
            var campaign = _context.Campaigns.First();
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
            var campaign = _context.Campaigns.First();

            // Act
            var result = await _service.DeleteCampaignById(campaign.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetOwnedCampaignsForUserById_ShouldReturnCampaigns_WhenUserOwnsCampaigns()
        {
            // Arrange
            var userId = _context.Users.First().Id;

            // Act
            var result = await _service.GetOwnedCampaignsForUserById(userId);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetCampaignsForUserById_ShouldReturnCampaigns_WhenUserIsParticipant()
        {
            // Arrange
            var userId = _context.Users.First().Id;

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
            var campaign = _context.Campaigns.First();
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
            // Arrange
            var session = new Session
            {
                Id = Guid.NewGuid(),
                Name = "New Session",
                CampaignId = _context.Campaigns.First().Id,
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
            var session = _context.Sessions.First();
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
            var session = _context.Sessions.First();

            // Act
            var result = await _service.DeleteSessionById(session.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetSessionById_ShouldReturnSession_WhenSessionExists()
        {
            // Arrange
            var sessionId = _context.Sessions.First().Id;

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
            var campaignId = _context.Campaigns.First().Id;

            // Act
            var result = await _service.GetSessionsForCampaign(campaignId);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetPaginatedSessionsForCampaign_ShouldReturnPaginatedSessions_WhenCampaignHasSessions()
        {
            // Arrange
            var campaignId = _context.Campaigns.First().Id;
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
            var session = _context.Sessions.First();
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
            var session = _context.Sessions.First();
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
            // Arrange
            var note = new Note
            {
                Id = Guid.NewGuid(),
                SessionId = _context.Sessions.Skip(1).First().Id, // Select the second session
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
            var noteId = _context.Notes.Skip(1).First().Id;

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
            var sessionId = _context.Sessions.First().Id;
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
            var note = _context.Notes.Skip(1).First();
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
            var note = _context.Notes.Skip(2).First();

            // Act
            var result = await _service.DeleteNoteById(note.Id);

            // Assert
            Assert.True(result);
        }   

    }
}
