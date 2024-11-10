using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CampaignManager.Persistence.Models
{
	//A campaign is a collection of sessions, players, and game masters
	//Players and game masters can be part of multiple campaigns
	//A campaign can have multiple sessions
	//The players and game masters are stored in the CampaignParticipants table
	public class Campaign
	{
		public int Id { get; set; }
		[Required]
		public string Name { get; set; } = string.Empty;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
		public string? OwnerId { get; set; } // This is the user ID of the owner of the campaign. It's nulalble because the owner can be deleted and the role can be transferred
		public virtual ApplicationUser? Owner { get; set; }
        public byte[]? Image { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
		public DateTime Edited { get; set; } = DateTime.Now;
        [Required]
        public DateTime GameTime { get; set; } ////The real world date of the campaign. We can create sessions with either past or future dates
        public virtual ICollection<CampaignParticipant> Participants { get; set; }
        public virtual ICollection<Session> Sessions { get; set; }
		public virtual ICollection<Invitation> Invitations { get; set; }
        //public virtual ICollection<NoteAdmin> NoteAdmins { get; set; }

        public Campaign()
		{
            Participants = new HashSet<CampaignParticipant>();
			Sessions = new HashSet<Session>();
			Invitations = new HashSet<Invitation>();
            //NoteAdmins = new HashSet<NoteAdmin>();
        }

	}
}
