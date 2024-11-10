/* using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    // This class represents the many-to-many relationship between users and campaigns for note administration.
    public class NoteAdmin
    {
        public Guid Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;

        public NoteAdmin()
        {
            Id = Guid.NewGuid();
        }
    }
} */