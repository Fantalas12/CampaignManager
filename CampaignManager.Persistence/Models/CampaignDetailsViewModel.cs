using System.Collections.Generic;

namespace CampaignManager.Persistence.Models
{
    public class CampaignDetailsViewModel
    {
        public Campaign Campaign { get; set; } = null!;
        public List<Session> Sessions { get; set; } = new List<Session>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}