using Microsoft.AspNetCore.Identity;

namespace CampaignManager.Persistence.Models
{

    //This class is used to represent a user in the application.
    public class ApplicationUser : IdentityUser
    {
		//Owned campaigns as campaign owner
		public virtual ICollection<Campaign> OwnedCampaigns { get; set; }

		//Owned sessions as a game master
		public virtual ICollection<Session> OwnedSessions { get; set; }

		// For campaigns that the user has joined with either role
		public virtual ICollection<CampaignParticipant> CampaignParticipants { get; set; }
		public virtual ICollection<SessionPlayer> SessionPlayers { get; set; }
        //For campaigns that the user has note admin role
        //public virtual ICollection<NoteAdmin> NoteAdmins { get; set; }
        public virtual ICollection<Generator> Generators { get; set; }
		public virtual ICollection<NoteType> NoteTypes { get; set; }
		public virtual ICollection<Template> Templates { get; set; }
        public virtual ICollection<Note> Notes { get; set; }



        public ApplicationUser()
		{
			OwnedCampaigns = new HashSet<Campaign>();
			OwnedSessions = new HashSet<Session>();
			CampaignParticipants = new HashSet<CampaignParticipant>();
			SessionPlayers = new HashSet<SessionPlayer>();
            //NoteAdmins = new HashSet<NoteAdmin>();
            Generators = new HashSet<Generator>();
            NoteTypes = new HashSet<NoteType>();
            Templates = new HashSet<Template>();
            Notes = new HashSet<Note>();
        }

	}
}
