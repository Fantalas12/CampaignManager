using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public class NoteDetailsViewModel
    {
        public Note Note { get; set; } = null!;
        //public List<Note> FromNotes { get; set; } = new List<Note>();
        //public List<Note> ToNotes { get; set; } = new List<Note>();
        //public int FromTotalCount { get; set; }
        //public int FromPage { get; set; }
        public List<NoteLink> ToNotes { get; set; } = new List<NoteLink>();
        public int ToTotalCount { get; set; }
        public int ToPage { get; set; }
        public int PageSize { get; set; }
    }

}
