using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace CampaignManager.Persistence.Models
{
    public class EditRoleViewModel
    {
        public Guid CampaignId { get; set; }
        [Required]
        public string ParticipantId { get; set; } = null!;
        [Required]
        public Role Role { get; set; }
        public List<SelectListItem> Participants { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }
}
