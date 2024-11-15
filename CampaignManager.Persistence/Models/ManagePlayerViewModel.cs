using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CampaignManager.Persistence.Models
{
    public class ManagePlayerViewModel
    {
        public Guid SessionId { get; set; } = Guid.Empty;

        [Required]
        [Display(Name = "Player")]
        public string SelectedPlayerId { get; set; } = null!;

        public List<SelectListItem> Players { get; set; } = new List<SelectListItem>();
    }
}