using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

using System;

namespace CampaignManager.Persistence.Models
{
    //Useful for creating a graph of notes including subnotes, tags etc.
    public class NoteLink
    {
        [Key]
        public Guid Id { get; set; }
        public Guid FromNoteId { get; set; }
        public virtual Note? FromNote { get; set; } 
        public Guid ToNoteId { get; set; }
        public virtual Note? ToNote { get; set; }
        public string? LinkType { get; set; }
        //public int Weight { get; set; }

        public NoteLink()
        {
            Id = Guid.NewGuid();
        }
    }
}
