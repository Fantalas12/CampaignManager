namespace CampaignManager.Persistence.Models
{
    public enum Role
    {
        Player,
        GameMaster,
        PlayerAndGameMaster
    }


    //This class represents the many-to-many relationship between users and campaigns...
    public class CampaignParticipant
    {
        public Guid Id { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
        public Guid CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;
        public Role Role { get; set; }
    }
}
