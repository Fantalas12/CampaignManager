using CampaignManager.Persistence.Models;
using System.Numerics;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CampaignManager.DTO;
using System.Security.Policy;
using System.Threading.Tasks;


namespace CampaignManager.Persistence.Services
{
    public interface ICampaignManagerService
    {
        // Campaign methods
        public Task<Campaign?> GetCampaignById(Guid campaignId);
        public Task<bool> AddCampaign(Campaign campaign);
        public Task<bool> UpdateCampaign(Campaign campaign);
        public Task<bool> DeleteCampaignById(Guid campaignId);
        public Task<List<Campaign>> GetOwnedCampaignsForUserById(string userId); // From Campaign table
        public Task<List<Campaign>> GetCampaignsForUserById(string userId);
        public Task<bool> SaveCampaignToFile(CampaignDTO campaignDto, string filePath);
        public Task<Campaign?> LoadCampaignFromFile(string filePath);
        public Task<bool> IsReservedCampaignNameForUser(string name, string userId);

        // CampaignParticipant methods
        public Task<bool> AddCampaignParticipant(CampaignParticipant participant);
        public Task<bool> UpdateCampaignParticipant(CampaignParticipant participant);
        public Task<bool> DeleteCampaignParticipantById(Guid participantId);
        public Task<List<CampaignParticipant>> GetPlayersForCampaign(Guid campaignId);
        public Task<List<CampaignParticipant>> GetGMsForCampaign(Guid campaignId);
        public Task<List<CampaignParticipant>> GetParticipantsForCampaign(Guid campaignId);
        public Task<CampaignParticipant?> GetParticipantForCampaignByUserId(string participantId, Guid campaignId);
        public Task<bool> IsUserParticipant(Guid campaignId, string userId);

        // Invitation methods
        public Task<bool> AddInvitation(Invitation invitation);
        public Task<bool> DeleteInvitationById(int invitationId);
        public Task<bool> DeleteOtherInvitationsForCampaign(Guid campaignId, string userId);
        public Task<List<Invitation>> GetInvitationsForUserById(string userId);
        public Task<Invitation?> GetInvitationById(int invitationId);

        // Session methods
        public Task<bool> AddSession(Session session);
        public Task<bool> UpdateSession(Session session);
        public Task<bool> DeleteSessionById(Guid sessionId);
        public Task<Session?> GetSessionById(Guid sessionId);
        public Task<List<Session>> GetSessionsForCampaign(Guid campaignId);
        public Task<(List<Session> Sessions, int TotalCount)> GetPaginatedSessionsForCampaign(Guid campaignId, int page, int pageSize);
        public Task<bool> IsReservedSessionNameForCampaign(string name, Guid campaignId);
        public Task<bool> IsReservedSessionDateForCampaign(DateTime date, Guid campaignId);

        // SessionPlayer methods
        public Task<bool> AddSessionPlayer(SessionPlayer sessionPlayer);
        public Task<bool> RemoveSessionPlayer(SessionPlayer sessionPlayer);

        // Note methods
        public Task<bool> AddNote(Note note);
        public Task<Note?> GetNoteById(Guid noteId);
        public Task<(List<Note> Notes, int TotalCount)> GetPaginatedNotesForSession(Guid sessionId, int page, int pageSize);
        public Task<bool> UpdateNote(Note note);
        public Task<bool> DeleteNoteById(Guid noteId);

        // NoteType methods
        public Task<bool> AddNoteType(NoteType noteType);
        public Task<NoteType?> GetNoteTypeById(Guid noteTypeId);
        public Task<(List<NoteType> NoteTypes, int TotalCount)> GetPaginatedNoteTypes(int page, int pageSize);
        public Task<bool> UpdateNoteType(NoteType noteType);
        public Task<bool> DeleteNoteTypeById(Guid noteTypeId);

        /*
        //AccessControlEntry methods
        public Task<bool> AddAccessControl(NoteAccess ace);
        public Task<NoteAccess?> GetAccessControlById(Guid aceId);
        public Task<bool> UpdateAccessControl(NoteAccess ace);
        public Task<bool> DeleteAccessControlById(Guid aceId);
        */

        // Template methods
        public Task<bool> AddTemplate(Template template);
        public Task<Template?> GetTemplateById(Guid templateId);
        public Task<(List<Template> Templates, int TotalCount)> GetPaginatedTemplates(int page, int pageSize);
        public Task<bool> UpdateTemplate(Template template);
        public Task<bool> DeleteTemplateById(Guid templateId);

        // Generator methods
        public Task<bool> AddGenerator(Generator generator);
        public Task<Generator?> GetGeneratorById(Guid generatorId);
        public Task<(List<Generator> Generators, int TotalCount)> GetPaginatedGenerators(int page, int pageSize);
        public Task<List<Generator>> GetPaginatedGeneratorsForNoteType(Guid noteTypeId, int page, int pageSize);
        public Task<List<Generator>> GetPaginaedGeneratorsForNote(Guid noteId, int page, int pageSize);
        public Task<bool> UpdateGenerator(Generator generator);
        public Task<bool> DeleteGeneratorById(Guid generatorId);

        // NoteLink methods

        public Task<bool> AddNoteLink(NoteLink noteLink);
        public Task<NoteLink?> GetNoteLinkById(Guid noteLinkId);
        Task<(List<NoteLink> NoteLinks, int TotalCount)> GetPaginatedToNoteLinksForNote(Guid noteId, int page, int pageSize);
        public Task<bool> UpdateNoteLink(NoteLink noteLink);
        public Task<bool> DeleteNoteLinkById(Guid noteLinkId);

        /*
        //NoteAdmin methods
        public Task<bool> AddNoteAdmin(NoteAdmin noteAdmin);
        public Task<List<NoteAdmin>> GetNoteAdminsForCampaign(int campaignId);
        public Task<NoteAdmin?> GetNoteAdminById(Guid noteAdminId);
        public Task<NoteAdmin?> GetNoteAdminByUserId(string userId, int campaignId);
        public Task<bool> UpdateNoteAdmin(NoteAdmin noteAdmin);
        public Task<bool> DeleteNoteAdminById(Guid noteAdminId);
        */

        //NoteGenerator methods
        public Task<bool> AddNoteGenerator(NoteGenerator noteGenerator);
        public Task<NoteGenerator?> GetNoteGeneratorById(Guid noteGeneratorId);
        public Task<(List<NoteGenerator> NoteGenerators, int TotalCount)> GetPaginatedNoteGenerators(int page, int pageSize);
        public Task<bool> UpdateNoteGenerator(NoteGenerator noteGenerator);
        public Task<bool> DeleteNoteGeneratorById(Guid noteGeneratorId);


        //Other methods
        public Task<IDbContextTransaction> BeginTransactionAsync();
        //Task TriggerNoteGeneratorNotification(NoteGenerator noteGenerator);


    }
}