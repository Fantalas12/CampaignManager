using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public class ChangeGameMasterViewModel
    {
        public Guid SessionId { get; set; } = Guid.Empty;
        public string? CurrentGameMasterId { get; set; }
        public string SelectedGameMasterId { get; set; } = string.Empty;
        public SelectList GameMasters { get; set; } = new SelectList(Enumerable.Empty<SelectListItem>()); // Initialize with an empty list
    }
}
