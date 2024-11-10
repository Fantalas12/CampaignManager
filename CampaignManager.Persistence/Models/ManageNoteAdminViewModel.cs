using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public class ManageNoteAdminViewModel
    {
        public int CampaignId { get; set; }
        [Required]
        public string SelectedParticipantId { get; set; } = null!;
        public SelectList? Participants { get; set; }
    }
}
