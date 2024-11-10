using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public class Template
    {
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        [DataType(DataType.MultilineText)]
        public string? Content { get; set; }
        public bool IsVerified { get; set; }
        public string? OwnerId { get; set; }
        public virtual ApplicationUser? Owner { get; set; }
        public virtual ICollection<NoteType> PlayerNoteTypes { get; set; }
        public virtual ICollection<NoteType> GameMasterNoteTypes { get; set; }
        /*
        public virtual ICollection<NoteType> PlayerViewNoteTypes { get; set; }
        public virtual ICollection<NoteType> GameMasterViewNoteTypes { get; set; }
        public virtual ICollection<NoteType> PlayerEditNoteTypes { get; set; }
        public virtual ICollection<NoteType> GameMasterEditNoteTypes { get; set; }
        */

        public Template()
        {
            Id = Guid.NewGuid();
            /*
            PlayerViewNoteTypes = new List<NoteType>();
            GameMasterViewNoteTypes = new List<NoteType>();
            PlayerEditNoteTypes = new List<NoteType>();
            GameMasterEditNoteTypes = new List<NoteType>();
            */
            PlayerNoteTypes = new List<NoteType>();
            GameMasterNoteTypes = new List<NoteType>();
            IsVerified = false;
        }
    }
}

