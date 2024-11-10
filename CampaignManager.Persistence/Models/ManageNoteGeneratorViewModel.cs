using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampaignManager.Persistence.Models
{
    public class ManageNoteGeneratorViewModel
    {
        public Guid Id { get; set; }
        public Guid? GeneratorId { get; set; }
        public Guid? NoteId { get; set; }
        public DateTime? NextRunInGameDate { get; set; }
    }
}
