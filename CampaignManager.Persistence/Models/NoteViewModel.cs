using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CampaignManager.Persistence.Models
{
    public class NoteViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        [JsonString]
        [DataType(DataType.MultilineText)]
        public string? Content { get; set; } // JSON-formatted text field
        public string? OwnerId { get; set; }
        public Guid? SessionId { get; set; }
        public DateTime InGameDate { get; set; } // The date which the note is relevant to the game
        public Guid? NoteTypeId { get; set; }
    }
}
