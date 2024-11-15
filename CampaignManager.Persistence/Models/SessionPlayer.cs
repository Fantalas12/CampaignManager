using Microsoft.VisualBasic;

namespace CampaignManager.Persistence.Models
{

    /* Many-to-many relationship between sessions and players...
    for providing extra information about the relationship */
    public class SessionPlayer
    {
        public int Id { get; set; }
        public Guid SessionId { get; set; } = Guid.Empty;
        public virtual Session Session { get; set; } = null!;
        public string ApplicationUserId { get; set; } = null!;
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
        public string SessonPlayerRole { get; set; } = null!;
        //public bool IsPresent { get; set; }

        // This is a nullable field because the player can be deleted
        //public string? PlayerNote { get; set; }

    }
}
