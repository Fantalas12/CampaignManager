/* using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public enum AccessLevel
    {
        Read,
        Write,
        Execute
    }

    // This class is used to control access to notes. It is a many-to-many relationship between notes and users.
    public class NoteAccess
    {
        public Guid Id { get; set; }
        public Guid? NoteId { get; set; }
        public virtual Note? Note { get; set; }
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public string AccessLevel { get; set; } //Read, Write, Execute, Admin

        public NoteAccess()
        {
            Id = Guid.NewGuid();
            AccessLevel = "Read";
        }
    }
} */