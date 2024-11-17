namespace CampaignManager.Persistence.Models
{
    public class Invitation
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
        public Guid CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;
        public Role Role { get; set; }
    }

}
