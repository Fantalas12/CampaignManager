﻿namespace CampaignManager.Persistence.Models
{
    public enum Role
    {
        Player,
        GameMaster,
        PlayerAndGameMaster
    }


    //This class represents the many-to-many relationship between users and campaigns...
    //Unifying the Player and GameMaster classes by using a Role property
    // TODO - Delete the Player and GameMaster classes
    public class CampaignParticipant
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
        public int CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;
        public Role Role { get; set; }
    }
}