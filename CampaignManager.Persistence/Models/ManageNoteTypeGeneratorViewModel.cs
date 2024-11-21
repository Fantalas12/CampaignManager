using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace CampaignManager.Persistence.Models
{
    public class ManageNoteTypeGeneratorViewModel
    {
        [Required]
        public Guid GeneratorId { get; set; }
        public Guid? NoteTypeId { get; set; }
    }
}
