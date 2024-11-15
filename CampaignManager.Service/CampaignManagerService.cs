using CampaignManager.Persistence.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Microsoft.Extensions.Logging;
using CampaignManager.DTO;
using System.Text;
using System.Text.Json;



//TODO - Implement generic repository pattern
namespace CampaignManager.Persistence.Services // TODO Change to CampaignManager.Service
{
    //This class is used for defining the methods which manipulate the data in the database.
    public class CampaignManagerService : ICampaignManagerService
    {

        private readonly CampaignManagerDbContext _context;
        //logger
        private readonly ILogger<CampaignManagerService> _logger;
        //private readonly HttpClient _httpClient;

        //public static event Action<NoteGeneratorDTO>? NoteGeneratorNotification;

        public CampaignManagerService(CampaignManagerDbContext context, ILogger<CampaignManagerService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Campaign methods

        public async Task<Campaign?> GetCampaignById(int campaignId)
        {
            return await _context.Campaigns
                .Include(c => c.Participants)
                    .ThenInclude(cp => cp.ApplicationUser)
                .Include(c => c.Sessions)
                    .ThenInclude(s => s.GameMaster)
                .Include(c => c.Sessions)
                    .ThenInclude(s => s.SessionPlayers)
                        .ThenInclude(sp => sp.ApplicationUser)
                .FirstOrDefaultAsync(c => c.Id == campaignId);
        }

        public async Task<bool> AddCampaign(Campaign campaign)
        {
            try
            {
                _context.Campaigns.Add(campaign);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateCampaign(Campaign campaign)
        {
            try
            {
                _context.Update(campaign);
                if (campaign.Image == null)
                {
                    _context.Entry(campaign).Property("Image").IsModified = false;
                }
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteCampaignById(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                return false;
            }

            try
            {
                _context.Campaigns.Remove(campaign);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        public async Task<List<Campaign>> GetOwnedCampaignsForUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<Campaign>();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new List<Campaign>();
            }

            return await _context.Campaigns
                .Where(c => c.OwnerId == user.Id)
                .ToListAsync();
        }

        public async Task<List<Campaign>> GetCampaignsForUserById(string userId)
        {
            return await _context.Campaigns
                .Include(c => c.Owner)
                .Include(c => c.Participants)
                .Where(c => c.OwnerId == userId || c.Participants.Any(p => p.ApplicationUserId == userId))
                .ToListAsync();
        }

        public async Task<bool> SaveCampaignToFile(CampaignDTO campaignDto, string filePath)
        {
            // Convert DTO to JSON and save to file
            var json = JsonConvert.SerializeObject(campaignDto);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }

        public async Task<Campaign?> LoadCampaignFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError($"File not found: {filePath}");
                    return null;
                }

                string json = await File.ReadAllTextAsync(filePath);
                CampaignDTO? campaignDto = JsonConvert.DeserializeObject<CampaignDTO>(json);

                if (campaignDto == null)
                {
                    _logger.LogError($"Failed to deserialize JSON from file: {filePath}");
                    return null;
                }

                Campaign campaign = (Campaign)campaignDto;
                return campaign;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading campaign from file");
                return null;
            }
        }

        public async Task<bool> IsReservedCampaignNameForUser(string name, string userId)
        {
            return await _context.Campaigns.
                AnyAsync(c => c.Name == name && c.OwnerId == userId);
        }

        #endregion

        #region CampaignParticipant methods

        public async Task<bool> AddCampaignParticipant(CampaignParticipant participant)
        {
            try
            {
                _context.Participants.Add(participant);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateCampaignParticipant(CampaignParticipant participant)
        {
            try
            {
                _context.Update(participant);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteCampaignParticipantById(int participantId)
        {
            CampaignParticipant? participant = await _context.Participants
                .FirstOrDefaultAsync(i => i.Id == participantId);

            if (participant == null)
                return false;

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<CampaignParticipant>> GetPlayersForCampaign(int campaignId)
		{
			return await _context.Participants
                .Where(p => p.CampaignId == campaignId && (p.Role == Role.Player || p.Role == Role.PlayerAndGameMaster))
                .Include(p => p.ApplicationUser)
                .ToListAsync();
		}

		public async Task<List<CampaignParticipant>> GetGMsForCampaign(int campaignId)
		{
			return await _context.Participants
                .Where(p => p.CampaignId == campaignId && (p.Role == Role.GameMaster || p.Role == Role.PlayerAndGameMaster	))
                .Include(p => p.ApplicationUser)
                .ToListAsync();
		}

        public async Task<List<CampaignParticipant>> GetParticipantsForCampaign(int campaignId)
        {
            return await _context.Participants
                .Where(p => p.CampaignId == campaignId)
                .Include(p => p.ApplicationUser)
                .ToListAsync();
        }

        public async Task<CampaignParticipant?> GetParticipantForCampaignByUserId(string participantId, int campaignId)
        {
            return await _context.Participants
                .Include(p => p.ApplicationUser)
                .Include(p => p.Campaign)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == participantId && p.CampaignId == campaignId);
        }

        public async Task<bool> IsUserParticipant(int campaignId, string userId)
        {
            return await _context.Participants
                .AnyAsync(cp => cp.CampaignId == campaignId && cp.ApplicationUserId == userId);
        }



        #endregion

        #region Invitation methods

        public async Task<bool> AddInvitation(Invitation invitation)
        {
            try
            {
                _context.Invitations.Add(invitation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

		public async Task<bool> DeleteInvitationById(int id) {
            var invitation = await _context.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return false;
            }

            try {
                _context.Invitations.Remove(invitation);
                await _context.SaveChangesAsync();
            } 
            catch (DbUpdateConcurrencyException)
            {
                   return false;
            }
            catch (DbUpdateException) 
            {
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteOtherInvitationsForCampaign(int campaignId, string userId)
        {
            try
            {
                var invitations = _context.Invitations
                    .Where(i => i.CampaignId == campaignId && i.ApplicationUserId == userId)
                    .ToList();

                _context.Invitations.RemoveRange(invitations);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                //_logger.LogError(ex, "Error deleting other invitations for campaign");
                return false;
            }
        }

        public async Task<List<Invitation>> GetInvitationsForUserById(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return new List<Invitation>();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return new List<Invitation>();
            }

            return await _context.Invitations
                .Where(c => c.ApplicationUserId == user.Id)
                .ToListAsync();
        }

        public async Task<Invitation?> GetInvitationById(int id) {
            return await _context.Invitations
                .Include(i => i.ApplicationUser)
                .Include(i => i.Campaign)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        #endregion

        #region Session methods
        
        public async Task<bool> AddSession(Session session)
        {
            try
            {
                _context.Sessions.Add(session);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> UpdateSession(Session session)
        {
            try
            {
                _context.Update(session);
                //Image handling if needed
                /* if (campaign.Image == null)
                {
                    _context.Entry(campaign).Property("Image").IsModified = false;
                } */
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteSessionById(Guid id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return false;
            }

            try
            {
                _context.Sessions.Remove(session);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        public async Task<Session?> GetSessionById(Guid sessionId)
        {
            return await _context.Sessions
                .Include(s => s.Campaign)
                .Include(s => s.GameMaster)
                .Include(s => s.SessionPlayers)
                    .ThenInclude(sp => sp.ApplicationUser)
                .FirstOrDefaultAsync(s => s.Id == sessionId);
        }

        public async Task<List<Session>> GetSessionsForCampaign(int campaignId)
        {
            return await _context.Sessions
                .Where(s => s.CampaignId == campaignId)
                .Include(s => s.GameMaster)
                .Include(s => s.SessionPlayers)
                    .ThenInclude(sp => sp.ApplicationUser)
                .ToListAsync();
        }

        public async Task<(List<Session> Sessions, int TotalCount)> GetPaginatedSessionsForCampaign(int campaignId, int page, int pageSize)
        {
            var query = _context.Sessions
                .Where(s => s.CampaignId == campaignId)
                .Include(s => s.GameMaster)
                .Include(s => s.SessionPlayers)
                    .ThenInclude(sp => sp.ApplicationUser);

            var totalCount = await query.CountAsync();
            var sessions = await query
                .OrderBy(s => s.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (sessions, totalCount);
        }

        public async Task<bool> IsReservedSessionNameForCampaign(string name, int campaignId)
        {
            return await _context.Sessions.
                AnyAsync(c => c.Name == name && c.CampaignId == campaignId);
        }

        public async Task<bool> IsReservedSessionDateForCampaign(DateTime date, int campaignId)
        {
            return await _context.Sessions.
                AnyAsync(c => c.Date == date && c.CampaignId == campaignId);
        }

        #endregion

        #region SessionPlayer methods

        public async Task<bool> AddSessionPlayer(SessionPlayer sessionPlayer)
        {
            try
            {
                _context.SessionPlayers.Add(sessionPlayer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        /*
        public async Task<bool> RemoveSessionPlayer(int sessionId, string userId)
        {
            var sessionPlayer = await _context.SessionPlayers
                .FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.ApplicationUserId == userId);

            if (sessionPlayer == null)
            {
                return false;
            }

            try
            {
                _context.SessionPlayers.Remove(sessionPlayer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        } */

        public async Task<bool> RemoveSessionPlayer(SessionPlayer sessionPlayer)
        {
            if (sessionPlayer == null)
            {
                return false;
            }

            try
            {
                _context.SessionPlayers.Remove(sessionPlayer);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Note methods

        public async Task<bool> AddNote(Note note)
        {
            try
            {
                _context.Notes.Add(note);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<Note?> GetNoteById(Guid noteId)
        {
            return await _context.Notes
                .Include(n => n.NoteType)
                .FirstOrDefaultAsync(n => n.Id == noteId);
        }

        public async Task<(List<Note> Notes, int TotalCount)> GetPaginatedNotesForSession(Guid sessionId, int page, int pageSize)
        {
            var query = _context.Notes
                .Where(n => n.SessionId == sessionId)
                .Include(n => n.NoteType);

            var totalCount = await query.CountAsync();
            var notes = await query
                //.OrderBy(n => n.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notes, totalCount);
        }

        public async Task<bool> UpdateNote(Note note)
        {
            try
            {
                _logger.LogInformation("Updating note with ID {NoteId}. Note details: {@Note}", note.Id, note);

                _context.Update(note);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated note with ID {NoteId}", note.Id);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error while updating note with ID {NoteId}", note.Id);
                return false;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error while updating note with ID {NoteId}", note.Id);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating note with ID {NoteId}", note.Id);
                return false;
            }

            return true;
        }


        public async Task<bool> DeleteNoteById(Guid noteId)
        {
            var note = await _context.Notes.FindAsync(noteId);
            if (note == null)
            {
                return false;
            }

            try
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        /*
        public async Task<IEnumerable<Note>> GetNotesForSession(int sessionId)
        {
            return await _context.Notes
                .Where(n => n.SessionId == sessionId)
                .Include(n => n.NoteType)
                .ToListAsync();
        } */

        /*
        public async Task<List<Note>> GetPaginatedNotesForSesson(Guid sessionId, int page, int pageSize)
        {
            return await _context.Notes
                .Where(n => n.SessionId == sessionId)
                .Include(n => n.NoteType)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        } */

        #endregion

        #region NoteType methods

        public async Task<bool> AddNoteType(NoteType noteType)
        {
            try
            {
                _context.NoteTypes.Add(noteType);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<NoteType?> GetNoteTypeById(Guid noteTypeId)
        {
            return await _context.NoteTypes
                .FirstOrDefaultAsync(nt => nt.Id == noteTypeId);
        }

        public async Task<(List<NoteType> NoteTypes, int TotalCount)> GetPaginatedNoteTypes(int page, int pageSize)
        {
            var query = _context.NoteTypes
                .Include(nt => nt.Owner);

            var totalCount = await query.CountAsync();
            var noteTypes = await query
                .OrderBy(nt => nt.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (noteTypes, totalCount);
        }

        public async Task<bool> UpdateNoteType(NoteType noteType)
        {
            try
            {
                _context.Update(noteType);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteNoteTypeById(Guid noteTypeId)
        {
            var noteType = await _context.NoteTypes.FindAsync(noteTypeId);
            if (noteType == null)
            {
                return false;
            }

            try
            {
                _context.NoteTypes.Remove(noteType);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        #endregion

        /*
        #region AccessControlEntry methods

        public async Task<bool> AddAccessControl(NoteAccess ace)
        {
            try
            {
                _context.NoteAccesses.Add(ace);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<NoteAccess?> GetAccessControlById(Guid aceId)
        {
            return await _context.NoteAccesses
                .FirstOrDefaultAsync(ace => ace.Id == aceId);
        }

        public async Task<bool> UpdateAccessControl(NoteAccess ace)
        {
            try
            {
                _context.Update(ace);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteAccessControlById(Guid aceId)
        {
            var ace = await _context.NoteAccesses.FindAsync(aceId);
            if (ace == null)
            {
                return false;
            }

            try
            {
                _context.NoteAccesses.Remove(ace);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        #endregion
        */

        #region Template methods

        public async Task<bool> AddTemplate(Template template)
        {
            try
            {
                _context.Templates.Add(template);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<Template?> GetTemplateById(Guid templateId)
        {
            return await _context.Templates
                .FirstOrDefaultAsync(t => t.Id == templateId);
        }

        public async Task<(List<Template> Templates, int TotalCount)> GetPaginatedTemplates(int page, int pageSize)
        {
            var query = _context.Templates
                .Include(t => t.Owner)
                .Include(t => t.PlayerNoteTypes)
                .Include(t => t.GameMasterNoteTypes);
                //.Include(t => t.PlayerViewNoteTypes)
                //.Include(t => t.GameMasterViewNoteTypes)
                //.Include(t => t.PlayerEditNoteTypes)
                //.Include(t => t.GameMasterEditNoteTypes);

            var totalCount = await query.CountAsync();
            var templates = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (templates, totalCount);
        }

        public async Task<bool> UpdateTemplate(Template template)
        {
            try
            {
                _context.Update(template);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteTemplateById(Guid templateId)
        {
            var template = await _context.Templates.FindAsync(templateId);
            if (template == null)
            {
                return false;
            }

            try
            {
                _context.Templates.Remove(template);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Generator methods

        public async Task<bool> AddGenerator(Generator generator)
        {
            try
            {
                _context.Generators.Add(generator);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<Generator?> GetGeneratorById(Guid generatorId)
        {
            return await _context.Generators
                .FirstOrDefaultAsync(g => g.Id == generatorId);
        }

        public async Task<(List<Generator> Generators, int TotalCount)> GetPaginatedGenerators(int page, int pageSize)
        {
            var query = _context.Generators
                .Include(g => g.Owner)
                .Include(g => g.NoteType);

            var totalCount = await query.CountAsync();
            var generators = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (generators, totalCount);
        }

        public async Task<List<Generator>> GetPaginatedGeneratorsForNoteType(Guid noteTypeId, int page, int pageSize)
        {
            var query = _context.Generators
                .Where(g => g.NoteTypeId == noteTypeId)
                .Include(g => g.Owner)
                .Include(g => g.NoteType);

            var totalCount = await query.CountAsync();
            var generators = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return generators;
        }

        public async Task<List<Generator>> GetPaginaedGeneratorsForNote(Guid noteId, int page, int pageSize)
        {
            var query = _context.Generators
                .Where(g => g.NoteId == noteId)
                .Include(g => g.Owner)
                .Include(g => g.NoteType);

            var totalCount = await query.CountAsync();
            var generators = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return generators;
        }


        public async Task<bool> UpdateGenerator(Generator generator)
        {
            try
            {
                _context.Update(generator);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteGeneratorById(Guid generatorId)
        {
            var generator = await _context.Generators.FindAsync(generatorId);
            if (generator == null)
            {
                return false;
            }

            try
            {
                _context.Generators.Remove(generator);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        #endregion

        #region NoteLink methods

        public async Task<bool> AddNoteLink(NoteLink noteLink)
        {
            try
            {
                _context.NoteLinks.Add(noteLink);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        
        public async Task<NoteLink?> GetNoteLinkById(Guid noteLinkId)
        {
            return await _context.NoteLinks
                .FirstOrDefaultAsync(nl => nl.Id == noteLinkId);
        }

        /*
        public async Task<(List<Note> Notes, int TotalCount)> GetPaginatedFromNotesForNote(Guid noteId, int page, int pageSize)
        {
            var query = _context.NoteLinks
                .Where(nl => nl.ToNoteId == noteId)
                .Select(nl => nl.FromNote)
                .Where(note => note != null);

            var totalCount = await query.CountAsync();
            var notes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notes.Where(note => note != null).Cast<Note>().ToList(), totalCount);
        }

        public async Task<(List<Note> Notes, int TotalCount)> GetPaginatedToNotesForNote(Guid noteId, int page, int pageSize)
        {
            var query = _context.NoteLinks
                .Where(nl => nl.FromNoteId == noteId)
                .Select(nl => nl.ToNote)
                .Where(note => note != null);

            var totalCount = await query.CountAsync();
            var notes = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (notes.Where(note => note != null).Cast<Note>().ToList(), totalCount);
        }
        */
        public async Task<(List<NoteLink> NoteLinks, int TotalCount)> GetPaginatedToNoteLinksForNote(Guid noteId, int page, int pageSize)
        {
            var query = _context.NoteLinks
                .Where(nl => nl.FromNoteId == noteId);

            var totalCount = await query.CountAsync();
            var noteLinks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (noteLinks, totalCount);
        }


        public async Task<bool> UpdateNoteLink(NoteLink noteLink)
        {
            try
            {
                _context.Update(noteLink);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteNoteLinkById(Guid noteLinkId)
        {
            var noteLink = await _context.NoteLinks.FindAsync(noteLinkId);
            if (noteLink == null)
            {
                return false;
            }

            try
            {
                _context.NoteLinks.Remove(noteLink);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        #endregion

        /*
        #region NoteAdmin methods

        public async Task<bool> AddNoteAdmin(NoteAdmin noteAdmin)
        {
            try
            {
                _context.NoteAdmins.Add(noteAdmin);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public Task<List<NoteAdmin>> GetNoteAdminsForCampaign(int campaignId)
        {
            return _context.NoteAdmins
                .Include(na => na.ApplicationUser)
                .Include(na => na.Campaign)
                .Where(na => na.CampaignId == campaignId)
                .ToListAsync();
        }

        public async Task<NoteAdmin?> GetNoteAdminById(Guid noteAdminId)
        {
            return await _context.NoteAdmins
                .Include(na => na.ApplicationUser)
                .Include(na => na.Campaign)
                .FirstOrDefaultAsync(na => na.Id == noteAdminId);
        }

        public async Task<NoteAdmin?> GetNoteAdminByUserId(string userId, int campaignId)
        {
            return await _context.NoteAdmins
                .Include(na => na.ApplicationUser)
                .Include(na => na.Campaign)
                .FirstOrDefaultAsync(na => na.ApplicationUserId == userId && na.CampaignId == campaignId);
        }

        public async Task<bool> UpdateNoteAdmin(NoteAdmin noteAdmin)
        {
            try
            {
                _context.Update(noteAdmin);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteNoteAdminById(Guid noteAdminId)
        {
            var noteAdmin = await _context.NoteAdmins.FindAsync(noteAdminId);
            if (noteAdmin == null)
            {
                return false;
            }

            try
            {
                _context.NoteAdmins.Remove(noteAdmin);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }


        #endregion
        */

        #region NoteGenerator methods

        public async Task<bool> AddNoteGenerator(NoteGenerator noteGenerator)
        {
            try
            {
                _context.NoteGenerators.Add(noteGenerator);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<NoteGenerator?> GetNoteGeneratorById(Guid noteGeneratorId)
        {
            return await _context.NoteGenerators
                .FirstOrDefaultAsync(ng => ng.Id == noteGeneratorId);
        }

        public async Task<(List<NoteGenerator> NoteGenerators, int TotalCount)> GetPaginatedNoteGenerators(int page, int pageSize)
        {
            var query = _context.NoteGenerators
                .Include(ng => ng.Note);

            var totalCount = await query.CountAsync();
            var noteGenerators = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (noteGenerators, totalCount);
        }

        public async Task<bool> UpdateNoteGenerator(NoteGenerator noteGenerator)
        {
            try
            {
                _context.Update(noteGenerator);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteNoteGeneratorById(Guid noteGeneratorId)
        {
            var noteGenerator = await _context.NoteGenerators.FindAsync(noteGeneratorId);
            if (noteGenerator == null)
            {
                return false;
            }

            try
            {
                _context.NoteGenerators.Remove(noteGenerator);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException)
            {
                return false;
            }
            return true;
        }

        #endregion


        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        /*
        public async Task TriggerNoteGeneratorNotification(NoteGenerator noteGenerator)
        {
            var noteGeneratorDTO = (NoteGeneratorDTO)noteGenerator;
            NoteGeneratorNotification?.Invoke(noteGeneratorDTO);
            await SendNotificationToApi(noteGeneratorDTO);
        } */

        /*
        private async Task SendNotificationToApi(NoteGeneratorDTO noteGeneratorDTO)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(noteGeneratorDTO);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("https://localhost:7148/api/notification/notify", content);
                //Log the response status code
                _logger?.LogInformation("Notification sent to the API. Status Code: {StatusCode}", response.StatusCode);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                _logger?.LogError(ex, "An error occurred while sending the notification to the API. Exception Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", ex.GetType(), ex.Message, ex.StackTrace);


                //Console.WriteLine($"Request error: {ex.Message}");
            }
        } */

    }
}
