using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CampaignManager.Persistence.Models
{

    // A session is a single game session that is part of a campaign
    // Not all players in the campaign need to be present in the session
    // A session can have multiple players
    // A session can only have a single game master from the possible game masters in the campaign
    public class Session
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.Date)]
        //The in real life datetime of the session
        //We can create sessions with either past or future dates
        public DateTime Date { get; set; }

        /*
        [Required]
        [DataType(DataType.Time)]
        public DateTime GameTime { get; set; } */

        //CampaignId is the ID of the campaign that the session is part of
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;

        //GameMasterId is the user ID of the game master for the session
        // Similar rights as the owner of the campaign
        public string? GameMasterId { get; set; } // This is nullable because the game master can be deleted and the role can be transferred
        public virtual ApplicationUser? GameMaster { get; set; }

        // Many-to-many relationship between sessions and users (as players)
        public virtual ICollection<SessionPlayer> SessionPlayers { get; set; }

        public virtual ICollection<Note> Notes { get; set; }

        public Session()
        {
            SessionPlayers = new List<SessionPlayer>();
            Notes = new List<Note>();
        }
    }
}
