using System;
using System.Collections.Generic;
using System.Text;
using CampaignManager.Persistence.Models;

namespace CampaignManager.DTO
{
    public class CampaignDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? OwnerId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Edited { get; set; }
        public DateTime GameTime { get; set; }
        public List<SessionDTO> Sessions { get; set; } = new List<SessionDTO>();
        public List<CampaignParticipantDTO> Participants { get; set; } = new List<CampaignParticipantDTO>();
        //public List<NoteAdminDTO> NoteAdmins { get; set; } = new List<NoteAdminDTO>();



        //Natural conversion between Campaign and CampaignDTO
        public static explicit operator Campaign(CampaignDTO campaignDTO) => new Campaign
        {
            Id = campaignDTO.Id,
            Name = campaignDTO.Name,
            Description = campaignDTO.Description,
            OwnerId = campaignDTO.OwnerId,
            Created = campaignDTO.Created,
            Edited = campaignDTO.Edited,
            GameTime = campaignDTO.GameTime,
            Sessions = campaignDTO.Sessions.Select(s => (Session)s).ToList(),
            Participants = campaignDTO.Participants.Select(p => (CampaignParticipant)p).ToList(),
            //NoteAdmins = campaignDTO.NoteAdmins.Select(n => (NoteAdmin)n).ToList()
        };

        public static explicit operator CampaignDTO(Campaign campaign) => new CampaignDTO
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            OwnerId = campaign.OwnerId,
            Created = campaign.Created,
            Edited = campaign.Edited,
            GameTime = campaign.GameTime,
            Sessions = campaign.Sessions.Select(s => (SessionDTO)s).ToList(),
            Participants = campaign.Participants.Select(p => (CampaignParticipantDTO)p).ToList(),
            //NoteAdmins = campaign.NoteAdmins.Select(n => (NoteAdminDTO)n).ToList()
        };



        //Conversion between Campaign and CampaignDTO with null campaignparticipant references
        public static Campaign ToCampaign(CampaignDTO campaignDTO)
        {
            return new Campaign
            {
                Id = campaignDTO.Id,
                Name = campaignDTO.Name,
                Description = campaignDTO.Description,
                OwnerId = null,
                Created = campaignDTO.Created,
                Edited = campaignDTO.Edited,
                GameTime = campaignDTO.GameTime,
                Sessions = campaignDTO.Sessions.Select(s => SessionDTO.ToSession(s)).ToList(),
                Participants = new List<CampaignParticipant>(), //Since the campaignparticipant references are null
                //NoteAdmins = new List<NoteAdmin>() //Since the noteadmin references are null
            };
        }

        public static CampaignDTO FromCampaign(Campaign campaign)
        {
            return new CampaignDTO
            {
                Id = campaign.Id,
                Name = campaign.Name,
                Description = campaign.Description,
                OwnerId = null,
                Created = campaign.Created,
                Edited = campaign.Edited,
                GameTime = campaign.GameTime,
                Sessions = campaign.Sessions.Select(s => SessionDTO.FromSession(s)).ToList(),
                Participants = new List<CampaignParticipantDTO>(), //Since the campaignparticipant references are null
                //NoteAdmins = new List<NoteAdminDTO>() //Since the noteadmin references are null
            };
        }

    }
}
