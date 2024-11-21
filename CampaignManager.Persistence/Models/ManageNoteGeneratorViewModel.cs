using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public class ManageNoteGeneratorViewModel
    {
        public Guid Id { get; set; }
        [Required]
        public Guid GeneratorId { get; set; }
        public Guid? NoteId { get; set; }
        public DateTime? NextRunInGameDate { get; set; }
    }
}
