using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CampaignManager.Persistence.Models
{
    public class NoteGenerator
    {
        public Guid Id { get; set; }
        public Guid? NoteId { get; set; }
        public virtual Note? Note { get; set; }
        public Guid? GeneratorId { get; set; }
        public virtual Generator? Generator { get; set; }
        public DateTime? NextRunInGameDate { get; set; }

        public NoteGenerator()
        {
            Id = Guid.NewGuid();
        }
    }
}