using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public class Generator
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }
        public string? Script { get; set; } // For user-defined generators
        public string? OwnerId { get; set; }
        public virtual ApplicationUser? Owner { get; set; }
        public Guid? NoteId { get; set; }
        public virtual Note? Note { get; set; }
        public Guid? NoteTypeId { get; set; }
        public virtual NoteType? NoteType { get; set; }
        public virtual ICollection<NoteGenerator> NoteGenerators { get; set; }
        public bool IsBuiltIn { get; set; }

        public Generator()
        {
            Id = Guid.NewGuid();
            Name = string.Empty;
            NoteGenerators = new List<NoteGenerator>();
            IsBuiltIn = false;
        }
    }
}
