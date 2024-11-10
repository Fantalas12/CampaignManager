using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CampaignManager.Persistence.Models;

namespace CampaignManager.DTO
{
    public class CampaignParticipantDTO
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public Role Role { get; set; }


        //Natural conversion between CampaignParticipant and CampaignParticipantDTO
        public static explicit operator CampaignParticipant(CampaignParticipantDTO campaignParticipantDTO) => new CampaignParticipant
        {
            Id = campaignParticipantDTO.Id,
            CampaignId = campaignParticipantDTO.CampaignId,
            ApplicationUserId = campaignParticipantDTO.ApplicationUserId,
            Role = campaignParticipantDTO.Role
        };

        public static explicit operator CampaignParticipantDTO(CampaignParticipant campaignParticipant) => new CampaignParticipantDTO
        {
            Id = campaignParticipant.Id,
            CampaignId = campaignParticipant.CampaignId,
            ApplicationUserId = campaignParticipant.ApplicationUserId,
            Role = campaignParticipant.Role
        };

    }
}
