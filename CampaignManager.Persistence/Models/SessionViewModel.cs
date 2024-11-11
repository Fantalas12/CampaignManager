using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace CampaignManager.Persistence.Models
{
    public class SessionViewModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        //[Required]
        //[DataType(DataType.DateTime)]
        //public DateTime GameTime { get; set; }

        public int CampaignId { get; set; }
        public string? GameMasterId { get; set; } 

        public static SessionViewModel FromSession(Session session)
        {
            return new SessionViewModel
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                Date = session.Date,
                //GameTime = session.GameTime,
                CampaignId = session.CampaignId,
                GameMasterId = session.GameMasterId
            };
        }

        public Session ToSession()
        {
            return new Session
            {
                Id = Id,
                Name = Name,
                Description = Description,
                Date = Date,
                //GameTime = GameTime,
                CampaignId = CampaignId,
                GameMasterId = GameMasterId
            };
        }

        public static explicit operator Session(SessionViewModel sessionViewModel)
        {
            return sessionViewModel.ToSession();
        }

        public static explicit operator SessionViewModel(Session session)
        {
            return FromSession(session);
        }
    }
}
