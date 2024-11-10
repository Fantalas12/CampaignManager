using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public class SessionDetailsViewModel
    {
        public Session Session { get; set; } = null!;
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<Note> Notes { get; set; } = new List<Note>();
        //public List<NoteAdmin> NoteAdmins { get; set; } = new List<NoteAdmin>();
    }

}
