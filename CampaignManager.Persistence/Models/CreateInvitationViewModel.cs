using Microsoft.AspNetCore.Mvc.Rendering;

namespace CampaignManager.Persistence.Models
{
    public class CreateInvitationViewModel
    {
        public int CampaignId { get; set; }
        public string Email { get; set; } = string.Empty;
        public Role Role { get; set; }
        public List<SelectListItem> Roles { get; set; } = new List<SelectListItem>();
    }

}
