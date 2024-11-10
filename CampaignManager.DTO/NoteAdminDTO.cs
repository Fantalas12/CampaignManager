/* using System;
using System.Collections.Generic;
using CampaignManager.Persistence.Models;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.DTO
{
    public class NoteAdminDTO
    {
        public Guid Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public int CampaignId { get; set; }

        // Conversion from NoteAdmin to NoteAdminDTO
        public static explicit operator NoteAdminDTO(NoteAdmin noteAdmin) => new NoteAdminDTO
        {
            Id = noteAdmin.Id,
            ApplicationUserId = noteAdmin.ApplicationUserId,
            CampaignId = noteAdmin.CampaignId
        };

        // Conversion from NoteAdminDTO to NoteAdmin
        public static explicit operator NoteAdmin(NoteAdminDTO noteAdminDTO) => new NoteAdmin
        {
            Id = noteAdminDTO.Id,
            ApplicationUserId = noteAdminDTO.ApplicationUserId,
            CampaignId = noteAdminDTO.CampaignId
        };
    }
}
*/