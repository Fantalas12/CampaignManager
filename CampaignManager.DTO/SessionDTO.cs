using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using CampaignManager.Persistence.Models;

namespace CampaignManager.DTO
{
    public class SessionDTO
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        //public DateTime GameTime { get; set; }
        public Guid CampaignId { get; set; }
        public string? GameMasterId { get; set; }
        public List<SessionPlayerDTO> SessionPlayers { get; set; } = new List<SessionPlayerDTO>();
        public List<NoteDTO> Notes { get; set; } = new List<NoteDTO>();

        //Natural conversion between Session and SessionDTO
        public static explicit operator Session(SessionDTO sessionDTO) => new Session
        {
            Id = sessionDTO.Id,
            Name = sessionDTO.Name,
            Description = sessionDTO.Description,
            Date = sessionDTO.Date,
            //GameTime = sessionDTO.GameTime,
            CampaignId = sessionDTO.CampaignId,
            GameMasterId = sessionDTO.GameMasterId,
            SessionPlayers = sessionDTO.SessionPlayers.Select(sp => (SessionPlayer)sp).ToList(),
            Notes = sessionDTO.Notes.Select(n => (Note)n).ToList()
        };

        public static explicit operator SessionDTO(Session session) => new SessionDTO
        {
            Id = session.Id,
            Name = session.Name,
            Description = session.Description,
            Date = session.Date,
            //GameTime = session.GameTime,
            CampaignId = session.CampaignId,
            GameMasterId = session.GameMasterId,
            SessionPlayers = session.SessionPlayers.Select(sp => (SessionPlayerDTO)sp).ToList(),
            Notes = session.Notes.Select(n => (NoteDTO)n).ToList()
        };

        //Conversion between Session and SessionDTO with null sessionplayer references
        public static Session ToSession(SessionDTO sessionDTO)
        {
            return new Session
            {
                Id = sessionDTO.Id,
                Name = sessionDTO.Name,
                Description = sessionDTO.Description,
                Date = sessionDTO.Date,
                //GameTime = sessionDTO.GameTime,
                CampaignId = sessionDTO.CampaignId,
                GameMasterId = null,
                SessionPlayers = new List<SessionPlayer>(), //Since the sessionplayer references are null
                Notes = sessionDTO.Notes.Select(n => NoteDTO.ToNote(n)).ToList(),
            };
        }

        public static SessionDTO FromSession(Session session)
        {
            return new SessionDTO
            {
                Id = session.Id,
                Name = session.Name,
                Description = session.Description,
                Date = session.Date,
                //GameTime = session.GameTime,
                CampaignId = session.CampaignId,
                GameMasterId = null,
                SessionPlayers = new List<SessionPlayerDTO>(), //Since the sessionplayer references are null
                //Notes = session.Notes.Select(n => (NoteDTO)n).ToList()
                Notes = session.Notes.Select(n => NoteDTO.FromNote(n)).ToList(),
            };
        }
        




    }

}
